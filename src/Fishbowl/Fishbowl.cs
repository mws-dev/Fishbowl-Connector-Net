using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FishbowlConnector
{
    public class Fishbowl : IDisposable
    {
        private string _key = "";

        private readonly string _user;
        private readonly string _pass;
        private readonly string _host;
        private readonly int _port;

        private ConnectionObject _connection;

        public Fishbowl(string host, int port, string user, string password)
        {
            _host = host;
            _port = port;
            _user = user;

            MD5 md5 = MD5.Create();
            byte[] encoded = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
            string md5Pass = Convert.ToBase64String(encoded, 0, 16);

            _pass = md5Pass;
        }

        public void Connect()
        {
            _connection = new ConnectionObject(_host, _port);
            Login();
        }

        private dynamic BeginRequest(dynamic _cmd)
        {
            dynamic cmd = new
            {
                FbiJson = new
                {
                    Ticket = new { Key = _key },
                    FbiMsgsRq = _cmd
                }
            };

            return cmd;
        }

        private void Login()
        {
            dynamic cmd = new { LoginRq = new { IAID = 3399 /* Application ID, can be any number */, IAName = "Application Name", IADescription = "Application Description", UserName = _user, UserPassword = _pass } };
            cmd = BeginRequest(cmd);
            string r = _connection.sendCommand(JsonConvert.SerializeObject(cmd));
            if (r == "")
                throw new Exception("Empty response returned for Login request");
            dynamic resp = JsonConvert.DeserializeObject(r);
            _key = resp.FbiJson.Ticket.Key;

            if ((resp.FbiJson.FbiMsgsRs.statusCode != 1000 && resp.FbiJson.FbiMsgsRs.statusCode != 900) || String.IsNullOrWhiteSpace(_key))
                throw new Exception("Login failed with status code " + resp.FbiJson.FbiMsgsRs.statusCode + (resp.FbiJson.FbiMsgsRs.statusMessage != null ? ": " + resp.FbiJson.FbiMsgsRs.statusMessage : ""));
            if (resp.FbiJson.FbiMsgsRs.LoginRs.statusCode != 1000)
                throw new Exception("Login Error " + resp.FbiJson.FbiMsgsRs.LoginRs.statusCode + ": " + resp.FbiJson.FbiMsgsRs.LoginRs.statusMessage.Value);
        }

        public DataTable ExecuteQuery(string query = "", string name = "")
        {
            dynamic cmd = new { ExecuteQueryRq = new { Name = name, Query = query } };
            cmd = BeginRequest(cmd);
            string r = _connection.sendCommand(JsonConvert.SerializeObject(cmd));
            if (r == "")
                throw new Exception("Empty response returned for Execute Query request");
            //File.WriteAllText(@"C:\tmp\cmd_resp.json", r);
            dynamic resp = JsonConvert.DeserializeObject(r);

            if (resp.FbiJson.FbiMsgsRs.statusCode != 1000 && resp.FbiJson.FbiMsgsRs.statusCode != 900)
                throw new Exception("Execute Query failed with status code " + resp.FbiJson.FbiMsgsRs.statusCode + (resp.FbiJson.FbiMsgsRs.statusMessage != null ? ": " + resp.FbiJson.FbiMsgsRs.statusMessage : ""));
            if (resp.FbiJson.FbiMsgsRs.ExecuteQueryRs.statusCode != 1000)
                throw new Exception("Execute Query Error " + resp.FbiJson.FbiMsgsRs.ExecuteQueryRs.statusCode + ": " + resp.FbiJson.FbiMsgsRs.ExecuteQueryRs.statusMessage.Value);

            return ConvertFromJson(resp);
        }

        public void Import(string type, dynamic data)
        {
            dynamic cmd = new { ImportRq = new { Type = type, Rows = new { Row = data } } };
            cmd = BeginRequest(cmd);
            //File.WriteAllText(@"C:\tmp\cmd.json", JsonConvert.SerializeObject(cmd));
            string r = _connection.sendCommand(JsonConvert.SerializeObject(cmd));
            //File.WriteAllText(@"C:\tmp\cmd_resp.json", r);
            if (r == "")
                throw new Exception("Empty response returned for Import request");
            dynamic resp = JsonConvert.DeserializeObject(r);

            if (resp.FbiJson.FbiMsgsRs.statusCode != 1000 && resp.FbiJson.FbiMsgsRs.statusCode != 900)
                throw new Exception("Import failed with status code " + resp.FbiJson.FbiMsgsRs.statusCode + (resp.FbiJson.FbiMsgsRs.statusMessage != null ? ": " + resp.FbiJson.FbiMsgsRs.statusMessage : ""));
            if (resp.FbiJson.FbiMsgsRs.ImportRs.statusCode != 1000)
                throw new Exception("Import Error " + resp.FbiJson.FbiMsgsRs.ImportRs.statusCode + ": " + resp.FbiJson.FbiMsgsRs.ImportRs.statusMessage.Value);
        }

        private DataTable ConvertFromJson(dynamic data)
        {
            DataTable t = new DataTable();

            _key = data.FbiJson.Ticket.Key;
            var rowData = data.FbiJson.FbiMsgsRs.ExecuteQueryRs.Rows.Row;

            string row = rowData.ToString().Trim('[', ']').Trim().Trim('"').Replace("\\\"", "\"").Replace("\",\r\n", "\r\n").Replace("\"\"", "\"");
            if (!row.EndsWith("\""))
            {
                row = row.Trim('\\');
                row += "\"";
            }

            using (var csv = new CsvReader(new StringReader(row), true, ','))
            {
                int fieldCount = csv.FieldCount;

                foreach (string c in csv.GetFieldHeaders())
                    t.Columns.Add(c);

                while (csv.ReadNextRecord())
                {
                    DataRow r = t.NewRow();
                    for (int i = 0; i < fieldCount; i++)
                        r[i] = csv[i];
                    t.Rows.Add(r);
                }
            }

            return t;
        }

        public static string ConvertDataTableToCsv(DataTable t)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataColumn c in t.Columns)
                sb.Append("\"" + c.ColumnName + "\",");
            sb.AppendLine();
            foreach (DataRow r in t.Rows)
            {
                foreach (DataColumn c in t.Columns)
                {
                    if (!String.IsNullOrWhiteSpace(r[c].ToString()))
                    {
                        if (r[c] is string)
                            sb.Append("\"" + r[c] + "\"");
                        else if (r[c] is int)
                            sb.Append(r[c].ToString());
                        else if (r[c] is decimal)
                            sb.AppendFormat("{0:0.####}", r[c]);
                    }
                    sb.Append(",");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            dynamic cmd = new { LogoutRq = "" };
            cmd = BeginRequest(cmd);

            dynamic resp = JsonConvert.DeserializeObject(_connection.sendCommand(JsonConvert.SerializeObject(cmd)));

            _connection.Dispose();
        }
    }
}
