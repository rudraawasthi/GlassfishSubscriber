using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GlassfishSubscriber
{
    public class DBClient
    {
        public static void StoreVerbatiamFileInDB(string connectionString, string filePath)
        {
           byte[] data = DiskIO.Read(filePath);
           StoreVerbatiamFileInDB(connectionString, data);
        }

        public static void StoreVerbatiamFileInDB(string connectionString, byte[] data)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            SqlParameter[] commandParameters = GetSqlParameters(data);

            using (SqlCommand command = new SqlCommand(DBConstants.SAVEVERBATIAMSTOREDPROC, connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
               
                command.Parameters.AddRange(commandParameters);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    TraceLogger.Log(e);
                    throw;
                }
            }
        }

        private static SqlParameter[] GetSqlParameters(byte[] data)
        {
            List<SqlParameter> storedProcParameters = new List<SqlParameter>();

            //TODO: Put in actual paramters

            SqlParameter parameter = new SqlParameter(DBConstants.SAVEVERBATIAMSTOREDPROCPARAMETER,data);
            
            storedProcParameters.Add(parameter);

            return storedProcParameters.ToArray();
        }
    }
}
