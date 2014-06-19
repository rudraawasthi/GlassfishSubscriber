using GlassfishSubscriber.resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GlassfishSubscriber
{
    /// <summary>
    /// Interacts with StompConnect instance on glassfish server
    /// </summary>
    public sealed class StompConnectClient : IDisposable
    {
        #region Consturctors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destination">destination on glassfish server</param>
        public StompConnectClient(Destination destination)
        {
            _destination = destination;
            Initialze();
        }

        #endregion

        /// <summary>
        /// Connects to a stompconnect instance
        /// </summary>
        public void Connect()
        {
            if (!IsDestinationSupported())
                throw new NotSupportedException(string.Format(NonLocalizableResource.DestinationNotSupported, _destination.Name, _destination.Destinationtype));

            if (_isConnected)
                return;

            try
            {
                string connectFrame = GetConnectFrame();

                byte[] connectFrameBytes = Encoding.Default.GetBytes(connectFrame);

                _binaryWriter.Write(connectFrameBytes);
                _binaryWriter.Flush();

                CheckConnectStatus();

                _isConnected = true;
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        ///  Subscribes to a glassfish destinations through stompconnect instance
        /// </summary>
        public List<STOMPMessage> Subscribe()
        {
            if (!IsDestinationSupported())
                throw new NotSupportedException(string.Format(NonLocalizableResource.DestinationNotSupported, _destination.Name, _destination.Destinationtype));

            try
            {
                if (!_isSubscribed)
                    SendSubscribe();

                List<STOMPMessage> messages = ReceiveAndProcessMessage();

                return messages;
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Stops receiving messages on stream
        /// </summary>
        public void UnsubscribeAndDisconnect()
        {
            _running = false;

            byte[] disconnectFrameBytes = Encoding.Default.GetBytes(StompMessageConstants.CONNECTFRAME);

            //to unblock the thread waiting for message on stream
            _binaryWriter.Write(disconnectFrameBytes);
            _binaryWriter.Flush();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (null != _binaryReader)
                _binaryReader.Dispose();

            if (null != _binaryWriter)
                _binaryWriter.Dispose();

            if (null != _networkStream)
                _networkStream.Dispose();

            if (null != _socket)
                _socket.Close();

            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
                Dispose();
        }

        public string DestinationName
        {
            get { return _destination.Name; }
        }

        #region Private Methods

        /// <summary>
        /// sends a subscribe frame to stompconnect instance
        /// </summary>
        private void SendSubscribe()
        {
            string destinationSpecificFrame = GetSubscribeFrame();

            string subscribeFrame = string.Format(destinationSpecificFrame, _destination.Name);
            byte[] subscribeFrameBytes = Encoding.Default.GetBytes(subscribeFrame);

            _binaryWriter.Write(subscribeFrameBytes);
            _binaryWriter.Flush();

            _isSubscribed = true;

            TraceLogger.Log(NonLocalizableResource.SubscribingToDestinationMessage, _destination.Name, _destination.Destinationtype);
        }

        /// <summary>
        /// Initializes tcp socket, network stream and binary reader writer objects
        /// </summary>
        private void Initialze()
        {
            try
            {
                _socket = new TcpClient(_destination.Host, _destination.Port);

                _networkStream = _socket.GetStream();

                _binaryWriter = new BinaryWriter(_networkStream);

                _binaryReader = new BinaryReader(_networkStream);
            }
            catch (Exception e)
            {
                TraceLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Checks status of connect message
        /// </summary>
        private void CheckConnectStatus()
        {
            string reply = GetConnectReply();

            if (string.IsNullOrEmpty(reply))
                throw new IOException(NonLocalizableResource.NoConnectReply);

            using (StringReader reader = new StringReader(reply))
            {
                string status = reader.ReadLine();

                if (status == null)
                    throw new IOException(NonLocalizableResource.NoConnectReply);

                if (status.Equals(StompMessageConstants.ERRORHEADER, StringComparison.InvariantCultureIgnoreCase))
                    throw new IOException(reply);
            }
        }

        /// <summary>
        /// Get reply of connect message from stompconnect instance
        /// </summary>
        /// <returns></returns>
        private string GetConnectReply()
        {
            List<byte> reply = new List<byte>();

            do
            {
                reply.Add(Convert.ToByte(_binaryReader.ReadByte()));

            } while (_networkStream.DataAvailable);

            string replyMessage = Encoding.Default.GetString(reply.ToArray());

            return replyMessage;
        }


        /// <summary>
        /// Waits for a message and process it after receiving
        /// </summary>
        /// <returns></returns>
        private List<STOMPMessage> ReceiveAndProcessMessage()
        {
            List<byte> reply = GetDataFromStream();

            if (!_running)
                return null;

            List<STOMPMessage> messages = STOMPMessage.GetMessages(reply);

            return messages;
        }


        /// <summary>
        /// Gets data from network stream
        /// </summary>
        /// <returns></returns>
        private List<byte> GetDataFromStream()
        {
            List<byte> reply = new List<byte>();
            byte dataByte;

            do
            {
                dataByte = Convert.ToByte(_binaryReader.ReadByte());
                reply.Add(dataByte);

            } while (_networkStream.DataAvailable && _running);

            //Necessary to wait till networkstream is populated with all the data;
            Thread.Sleep(5000); //wait 5 seconds;

            while (_networkStream.DataAvailable && _running)
            {
                dataByte = Convert.ToByte(_binaryReader.ReadByte());
                reply.Add(dataByte);
            }

            return reply;
        }

        /// <summary>
        /// Gets a connect frame
        /// </summary>
        /// <returns></returns>
        private string GetConnectFrame()
        {
            string connectFrame = (string.IsNullOrEmpty(_destination.UserName)) ? StompMessageConstants.CONNECTFRAME :
                                              string.Format(StompMessageConstants.CONNECTFRAMEWITHCREDENTIALS, _destination.UserName, _destination.Password);

            return connectFrame;
        }

        /// <summary>
        /// Gets a subscribe frame
        /// </summary>
        /// <returns></returns>
        private string GetSubscribeFrame()
        {
            string subscribeFrame = (AppConfigConstants.TOPICDESTINATIONTYPE.Equals(_destination.Destinationtype, StringComparison.OrdinalIgnoreCase))
                                        ? StompMessageConstants.SUBSCRIBETOPICFRAME : StompMessageConstants.SUBSCRIBEQUEUEFRAME;

            return subscribeFrame;
        }

        private bool IsDestinationSupported()
        {
            return (AppConfigConstants.TOPICDESTINATIONTYPE.Equals(_destination.Destinationtype, StringComparison.OrdinalIgnoreCase) ||
                 AppConfigConstants.QUEUEDESTINATIONTYPE.Equals(_destination.Destinationtype, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Private Members

        private NetworkStream _networkStream;
        private TcpClient _socket;

        private BinaryWriter _binaryWriter;
        private BinaryReader _binaryReader;

        private volatile bool _running = true;

        private Destination _destination;

        private bool _isSubscribed = false;
        private bool _isConnected = false;

        #endregion
    }

}
