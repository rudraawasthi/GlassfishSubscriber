using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlassfishSubscriber
{
    public sealed class SubscriberFacade : IDisposable
    {
        #region Constuctors

        public SubscriberFacade()
            : this(GlassfishSubscriberConfiguration.SubscriberConfiguration, StompConnectConfiguration.StompConfiguration, DBConfiguration.ConnectionString)
        {
        }

        public SubscriberFacade(GlassfishSubscriberConfiguration glassfishSubscriberconfiguration, StompConnectConfiguration stompConnectConfiguration, string connectionString)
        {
            _glassfishSubscriberconfiguration = glassfishSubscriberconfiguration;
            _stompConnectConfiguration = stompConnectConfiguration;
            _dbConnectionString = connectionString;

            if (_glassfishSubscriberconfiguration.StorageConfiguration.SaveOnDisk)
                DiskIO.CreateTempDirectory(_glassfishSubscriberconfiguration.StorageConfiguration.StorageLocation);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Connects and subscribes to destinations
        /// </summary>
        public void Subscribe()
        {
            try
            {
                _stompConnectSubscriber.Subscribe(_stompConnectConfiguration.Destinations, SubscribeCallBack);
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }


        /// <summary>
        /// Stops receiving messages
        /// </summary>
        public void Unsubscribe()
        {
            try
            {              
                _stompConnectSubscriber.Unsubscribe();
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
            finally
            {
                if (_glassfishSubscriberconfiguration.StorageConfiguration.SaveOnDisk)
                    DiskIO.DeleteTemporaryDirectory(_glassfishSubscriberconfiguration.StorageConfiguration.StorageLocation);
            }
        }

        public void Dispose()
        {
            _stompConnectSubscriber.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles received messages
        /// </summary>
        /// <param name="asyncResult"></param>
        private void SubscribeCallBack(List<STOMPMessage> stompMessageList, string destinationName)
        {  
            try
            {
                if (null == stompMessageList)
                {
                    Dispose();
                    return;
                }
                //call bl here
               Task.Factory.StartNew(() => MessageProcessor.ProcessAsync(stompMessageList, destinationName, _glassfishSubscriberconfiguration.StorageConfiguration, _dbConnectionString));
            }          
            catch (Exception ex)
            {
                TraceLogger.Log(ex);              
            }
        }

        #endregion

        #region PrivateMembers

        private GlassfishSubscriberConfiguration _glassfishSubscriberconfiguration;

        private StompConnectConfiguration _stompConnectConfiguration;

        private readonly string _dbConnectionString;

        private StompConnectSubscriber _stompConnectSubscriber = new StompConnectSubscriber();

        #endregion
    }
}
