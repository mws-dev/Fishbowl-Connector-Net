using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace FishbowlConnector
{
    /// <summary>
    /// Summary description for FishbowlStatus
    /// </summary>
    internal static class FishbowlStatus
    {
        private static readonly ReadOnlyDictionary<int, string> _list = new ReadOnlyDictionary<int, string>(new Dictionary<int, string>
        {
            { 900, "Success - This API request is deprecated and expected to be removed soon." },
            { 1000, "Success" },
            { 1002, "Connection to Fishbowl server was lost" },
            { 1003, "Some requests had errors" },
            { 1004, "There was an error with the database" },
            { 1009, "Fishbowl server has been shut down" },
            { 1010, "You have been logged off the server by an administrator" },
            { 1014, "Unable to establish network connection" },
            { 1100, "Unknown login error occurred" },
            { 1109, "This integrated application registration key is already in use" },
            { 1110, "A new integrated application has been added to Fishbowl. Please contact the Fishbowl administrator to approve this integrated application" },
            { 1111, "This integrated application registration key does not match" },
            { 1112, "This integrated application has not been approved by the Fishbowl administrator" },
            { 1120, "Invalid username or password" },
            { 1130, "Invalid ticket passed to Fishbowl server" },
            { 1131, "Invalid ticket key passed to Fishbowl server" },
            { 1162, "The login limit has been reached for the server's key" },
            { 1164, "Your API session has been logged out" }
        });
    }
}