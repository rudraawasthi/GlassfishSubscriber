using GlassfishSubscriber.resource;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlassfishSubscriber
{
    public class StompConnectSubscriber : IDisposable
    {

        #region Public Methods

        /// <summary>
        /// Connects and subscribes to destinations on glassfish server through stompconnect instance
        /// </summary>
        public void Subscribe(DestinationCollection destinations, Action<List<STOMPMessage>, string> subscribeCallback)
        {
            _subscribeCallback = subscribeCallback;
            try
            {
                Parallel.For(0, destinations.Count, i =>
                {
                    StompConnectClient stompConnectClient = new StompConnectClient(destinations[i]);

                    _stompClients.Add(stompConnectClient);

                    ConnectAndSubscribe(stompConnectClient);
                });
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Stops receiving messages and dispose underlying objects
        /// </summary>
        public void Unsubscribe()
        {
            try
            {
                _running = false;
                Parallel.For(0, _stompClients.Count, i =>
                                                { _stompClients[i].UnsubscribeAndDisconnect(); }
                            );
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
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

        public void Dispose(bool disposing)
        {
            if (disposing)
                Dispose();
        }

        #endregion

        #region Private Methods

        private void ConnectAndSubscribe(StompConnectClient client)
        {
            client.Connect();
            Subscribe(client);
        }

        private void Subscribe(StompConnectClient client)
        {
            if (!_running)
                return;

            if (null == client)
            {
                TraceLogger.LogWarning(NonLocalizableResource.StompClientNullMessage);
                return;
            }

            SubscribeCallback subscriber = new SubscribeCallback(client.Subscribe);
            AsyncCallback callback = new AsyncCallback(GetMessages);

            IAsyncResult result = subscriber.BeginInvoke(callback, subscriber);
        }

        /// <summary>
        /// Handles received messages
        /// </summary>
        /// <param name="asyncResult"></param>
        private void GetMessages(IAsyncResult asyncResult)
        {
            SubscribeCallback subscriber = asyncResult.AsyncState as SubscribeCallback;

            if (null == subscriber)
                return;

            StompConnectClient client = subscriber.Target as StompConnectClient;

            try
            {
                List<STOMPMessage> messages = subscriber.EndInvoke(asyncResult);

                if (null == messages || !_running)
                {
                    DisposeClient(client);
                    return;
                }

                if (null != _subscribeCallback)
                    _subscribeCallback(messages, client.DestinationName);

                //Go back and receive messages;
                if (null != messages)
                    Subscribe(client);
            }
            catch (ArgumentException ae)
            {
                TraceLogger.Log(ae);
                Subscribe(client);
            }
            catch (Exception ex)
            {
                TraceLogger.Log(ex);
                client.Dispose();
            }
        }

        private void DisposeClient(StompConnectClient client)
        {
            if (null == client)
                return;

            client.Dispose();

            if (_stompClients.Contains(client))
                _stompClients.Remove(client);
        }

        #endregion

        #region PrivateMembers

        private List<StompConnectClient> _stompClients = new List<StompConnectClient>();

        private delegate List<STOMPMessage> SubscribeCallback();

        private volatile bool _running = true;

        private Action<List<STOMPMessage>, string> _subscribeCallback;

        #endregion
    }
}
