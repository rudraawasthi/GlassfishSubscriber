using GlassfishSubscriber.resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GlassfishSubscriber
{
    public sealed class GlassfishSubscriber : IDisposable
    {
        #region Constuctors

        public GlassfishSubscriber()
            : this(GlassfishSubscriberConfiguration.SubscriberConfiguration, StompConnectConfiguration.StompConfiguration, DBConfiguration.ConnectionString)
        {
        }

        public GlassfishSubscriber(GlassfishSubscriberConfiguration glassfishSubscriberconfiguration, StompConnectConfiguration stompConnectConfiguration, string connectionString)
        {
            _glassfishSubscriberconfiguration = glassfishSubscriberconfiguration;
            _stompConnectConfiguration = stompConnectConfiguration;
            _dbConnectionString = connectionString;

            if (_glassfishSubscriberconfiguration.StorageConfiguration.SaveOnDisk)
                _tempDirectory = DiskIO.CreateTempDirectory(_glassfishSubscriberconfiguration.StorageConfiguration.StorageLocation);
        }

        #endregion

        #region Public Members


        /// <summary>
        /// Connects and subscribes to destinations on glassfish server through stompconnect instance
        /// </summary>
        public void Subscribe()
        {
            try
            {
                StompConnectSubscriber stompConnectSubscriber = new StompConnectSubscriber();
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }


        /// <summary>
        /// Stops receiving messages and dispose underlying
        /// </summary>
        public void Unsubscribe()
        {
            try
            {
                _running = false;
                Parallel.For(0, _stompClients.Count, i => { _stompClients[i].UnsubscribeAndDisconnect(); });
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
            foreach (StompConnectClient client in _stompClients)
            {
                if (null != client)
                    client.Dispose();
            }
        }

        #endregion

        #region PrivateMembers

        private List<StompConnectClient> _stompClients = new List<StompConnectClient>();

        private GlassfishSubscriberConfiguration _glassfishSubscriberconfiguration;

        private StompConnectConfiguration _stompConnectConfiguration;

        private delegate List<STOMPMessage> Subscriber();

        private delegate void Connector();

        private volatile bool _running = true;

        private readonly string _tempDirectory;

        private readonly string _dbConnectionString;

        #endregion
    }
}
