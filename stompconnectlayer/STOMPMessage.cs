using System;
using System.Collections.Generic;
using System.Text;

namespace GlassfishSubscriber
{
    /// <summary>
    /// Handles messages received from stompconnect instance
    /// </summary>
    public class STOMPMessage
    {        
        public STOMPMessage(MessageMatadata metadata)
        {
            _metadata = metadata;
        }

        public STOMPMessage(MessageMatadata metadata, byte[] data)
        {
            _metadata = metadata;
            Data = data;
        }

        public byte[] Data
        { 
            get; set; 
        }

        public MessageMatadata Metadata
        { 
            get { return _metadata; } 
        }

          /// <summary>
        /// Processes all the messages received
        /// </summary>
        /// <param name="allData">messages as bytes</param>
        /// <returns>List of messages</returns>
        public static List<STOMPMessage> GetMessages(List<byte> allData)
        {
            List<STOMPMessage> messageList = new List<STOMPMessage>();

            while (allData.Count > StompMessageConstants.TENTATIVEMESSAGEHEADERREADABLELENGTH)
            {
                STOMPMessage message = GetMessage(allData);
                messageList.Add(message);
                RemoveProcessedBytes(ref allData, message.Metadata.MessageEndIndex);
            }

            return messageList;
        }
      
        private MessageMatadata _metadata;
      
        #region Private Methods

        /// <summary>
        /// Gets message metadata and content from the stream of bytes
        /// </summary>
        /// <param name="allData">received bytes from stream</param>
        /// <returns>message object</returns>
        private static STOMPMessage GetMessage(List<byte> allData)
        {
            MessageMatadata messageMetadata = GetMessageMetadata(allData);

            byte[] messageContent = allData.GetRange(messageMetadata.ContentStartIndex, messageMetadata.ContentLength).ToArray();

            STOMPMessage message = new STOMPMessage(messageMetadata, messageContent);

            return message;
        }

        private static MessageMatadata GetMessageMetadata(List<byte> allData)
        {
            MessageMatadata messageMetadata = new MessageMatadata();

            string messageHeader = GetMessageHeader(allData);

            messageMetadata.MessageType = GetMessageType(messageHeader);

            messageMetadata.ContentStartIndex = GetContentStartIndex(messageHeader);

            messageMetadata.ContentLength = GetContentLength(messageHeader);

            if (messageMetadata.ContentLength == -1)// only for text message when content-length header is missing
            {
                messageMetadata.MessageEndIndex = GetEndIndexForMessage(allData.ToArray());
                messageMetadata.ContentLength = messageMetadata.MessageEndIndex - messageMetadata.ContentStartIndex;
                messageMetadata.MessageType = AppConfigConstants.TEXTMESSAGECONTENTTYPE; //set file extension as text
            }
            else
                messageMetadata.MessageEndIndex = messageMetadata.ContentStartIndex + messageMetadata.ContentLength + 2;

            return messageMetadata;
        }

        private static string GetMessageType(string messageHeader)
        {
            string contentType = GetMessageHeaderValue(messageHeader, StompMessageConstants.CONTENTTYPECUSTOMHEADER);

            if (string.IsNullOrEmpty(contentType))
                contentType = GetMessageHeaderValue(messageHeader, StompMessageConstants.TYPEHEADER);

            if (string.IsNullOrEmpty(contentType))
                return AppConfigConstants.DEFAULTMESSAGECONTENTTYPE;

            if (contentType.StartsWith(".")) //remove DOT from message type;
                contentType = contentType.Remove(0, 1);

            return contentType;
        }


        private static string GetMessageHeaderValue(string messageHeader, string headerKey)
        {
            string headerValue = string.Empty;

            string header = string.Concat(headerKey, StompMessageConstants.HEADERKEYVALUESEPARATOR);

            int headerStartIndex = messageHeader.IndexOf(header, StringComparison.OrdinalIgnoreCase);

            if (-1 == headerStartIndex) // content-length header is only present in binary data messages
                return headerValue;

            int headerEndIndex = messageHeader.IndexOf(StompMessageConstants.ENDOFHEADER, headerStartIndex, StringComparison.InvariantCultureIgnoreCase);

            headerValue = messageHeader.Substring((headerStartIndex + header.Length), (headerEndIndex - headerStartIndex - header.Length));

            return headerValue;
        }

        private static string GetMessageHeader(List<byte> allData)
        {
            byte[] messageHeaders = allData.GetRange(0, StompMessageConstants.TENTATIVEMESSAGEHEADERREADABLELENGTH).ToArray();

            return Encoding.Default.GetString(messageHeaders);
        }

        /// <summary>
        /// Removes the processed message data along with stompconnect headers
        /// </summary>
        /// <param name="allData"></param>
        /// <param name="messageEndIndex"></param>
        private static void RemoveProcessedBytes(ref List<byte> allData, int messageEndIndex)
        {
            if (StompMessageConstants.TENTATIVEMINIMUMMESSAGEHEADERLENGTH > (allData.Count - messageEndIndex))
            {
                allData.Clear();
                return;
            }

            string s = Encoding.Default.GetString(allData.GetRange(messageEndIndex, 50).ToArray());

            int removalIndex = s.IndexOf(StompMessageConstants.ENDOFMESSAGECONTENT) + 2 + messageEndIndex;

            if (allData.Count > removalIndex)
                allData.RemoveRange(0, removalIndex);
            else
                allData.Clear();

        }

        /// <summary>
        /// Get the index where message data begins
        /// </summary>
        /// <param name="messageHeaders"></param>
        /// <returns></returns>
        private static int GetContentStartIndex(string messageHeader)
        {
            return (messageHeader.IndexOf(StompMessageConstants.ENDOFMESSAGEHEADERS) + StompMessageConstants.ENDOFMESSAGEHEADERS.Length);
        }

        /// <summary>
        /// Get the index where message data ends
        /// </summary>
        /// <param name="data">message data</param>
        /// <returns></returns>
        private static int GetEndIndexForMessage(byte[] data)
        {
            //reading message in chunks of 1000 bytes to determine eom           
            int readCursor = 0;

            int remainingChars = data.Length;
            int endIndex = 0;

            while (readCursor < data.Length)
            {
                if (remainingChars > StompMessageConstants.READINCHUNKSLENGTH)
                {
                    string s = Encoding.Default.GetString(data, readCursor, StompMessageConstants.READINCHUNKSLENGTH);

                    endIndex = s.IndexOf(StompMessageConstants.ENDOFMESSAGECONTENT) + 1;

                    if (0 != endIndex)
                        break;

                    readCursor += StompMessageConstants.READINCHUNKSLENGTH;
                    remainingChars -= StompMessageConstants.READINCHUNKSLENGTH;
                }
                else
                {
                    string s = Encoding.Default.GetString(data, readCursor - StompMessageConstants.READINCHUNKSLENGTH, remainingChars);
                    return s.IndexOf(StompMessageConstants.ENDOFMESSAGECONTENT) + 1;
                }
            }

            return endIndex + readCursor;
        }

        /// <summary>
        /// Get the length of message data
        /// </summary>
        /// <param name="messageHeaders">stomp connect headers in message</param>
        /// <returns></returns>
        private static int GetContentLength(string messageHeader)
        {
            string contentLength = GetMessageHeaderValue(messageHeader, StompMessageConstants.CONTENTLENGTHHEADER);

            if (string.IsNullOrEmpty(contentLength))
                return -1;

            int length;

            if (int.TryParse(contentLength, out length))
                return length;
            else
                throw new ArithmeticException("Unable to convert content length in message header into integer");
        }

        #endregion
    }

    /// <summary>
    /// Provides metadata for message
    /// </summary>
    public class MessageMatadata
    {
        public int ContentStartIndex
        { get; set; }

        public int ContentLength
        { get; set; }

        public int MessageEndIndex
        { get; set; }

        public string MessageType
        { get; set; }
    }   
}
