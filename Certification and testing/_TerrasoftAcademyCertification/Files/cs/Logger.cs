using System;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace ExternalSystemsIntegration.Files.cs
{
    public static class Logger
    {
        public static bool WriteToLog(string service, string body, UserConnection userConnection)
        {
            try
            {
                Insert insert = new Insert(userConnection).Into("UsrLog")
                .Set("UsrExecution", Column.Parameter(DateTime.Now))
                .Set("UsrService", Column.Parameter(service))
                .Set("UsrBody", Column.Parameter(body));

                insert.Execute();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}