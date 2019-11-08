using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FishbowlConnector
{
    internal class ConnectionObject : IDisposable
    {
        private TcpClient tc;
        private NetworkStream tcS;

        private EndianBinaryWriter bw;
        private EndianBinaryReader br;

        private Socket sock;
        private IPEndPoint ipep;
        private IPAddress ipAddr;

        /**
         * Create the server connection
         */
        public ConnectionObject(string host, int port)
        {
            tc = new TcpClient(host, port);
            tcS = tc.GetStream();

            bw = new EndianBinaryWriter(new BigEndianBitConverter(), tcS);
            br = new EndianBinaryReader(new BigEndianBitConverter(), tcS);
        }

        /**
         * Send the XML request string
         */
        internal String sendCommand(string command)
        {
            try
            {
                bw = new EndianBinaryWriter(new BigEndianBitConverter(), tcS);
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(command);
                bw.Write(bytes.Length);
                bw.Write(bytes);
                bw.Flush();
                Thread.Sleep(2000); // required for server to process before sending response
                br = new EndianBinaryReader(new BigEndianBitConverter(), tcS);
                int i = br.ReadInt32();
                byte[] bytess = new byte[i];
                br.Read(bytess, 0, i);
                String response = encoding.GetString(bytess, 0, i);

                return response;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void Dispose()
        {
            tcS.Dispose();
            tc.Close();
        }
    }
}
