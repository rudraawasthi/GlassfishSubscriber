using GlassfishSubscriber.resource;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlassfishSubscriber
{
    public class MessageProcessor
    {
        #region Public Methods

        public static async void ProcessAsync(List<STOMPMessage> messageList, string destinationName, StorageConfiguration storageConfiguration, string dbConnectionString)
        {
            try
            {
                if (!string.IsNullOrEmpty(dbConnectionString))
                    await Task.Factory.StartNew(() => InsertToDatabase(messageList, dbConnectionString));

                if (storageConfiguration.SaveOnDisk)                
                    await SaveMessagesAsynchronously(messageList, destinationName, storageConfiguration);                    
                
                //Task.Factory.StartNew(() => SaveMessages(messageList, destinationName, storageConfiguration));
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
            }
        }

        #endregion

        #region Private Methods

        private static void InsertToDatabase(List<STOMPMessage> messageList, string dbConnectionString)
        {
            try
            {
                STOMPMessage message = messageList.Find(m => m.Metadata.MessageType.Equals("wav", StringComparison.InvariantCultureIgnoreCase));

                InsertVerbatiamFile(message.Data, dbConnectionString);
                // Task.Factory.StartNew(() => InsertVerbatiamFile(message.Data));
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
            }
        }

        private static void InsertVerbatiamFile(byte[] data, string dbConnectionString)
        {
            DBClient.StoreVerbatiamFileInDB(dbConnectionString, data);
        }

        /// <summary>
        /// Saves messages to disk
        /// </summary>
        /// <param name="messages">message</param>
        /// <param name="destinationName">destination name</param>
        /// <returns>list of saved files</returns>
        private static List<string> SaveMessages(List<STOMPMessage> messages, string destinationName, StorageConfiguration storageConfiguration)
        {
            TraceLogger.Log(NonLocalizableResource.SavingMessageFromDestiantion, messages.Count, destinationName);

            List<string> files = new List<string>();

            Parallel.ForEach(messages, message =>
            {
                string filePath = DiskIOHelper.GetTempFileFullName(storageConfiguration.StorageLocation, storageConfiguration.BaseFileName, destinationName, message.Metadata.MessageType);
                DiskIO.Write(filePath, message.Data);

                files.Add(filePath);
            });

            return files;
        }

        private static Task SaveMessagesAsynchronously(List<STOMPMessage> messages, string destinationName, StorageConfiguration storageConfiguration)
        {
            var task = Task.Factory.StartNew(() => SaveMessages(messages, destinationName, storageConfiguration));
            return task;
        }

        #endregion
    }
}
