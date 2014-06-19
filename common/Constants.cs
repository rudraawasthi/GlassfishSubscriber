
namespace GlassfishSubscriber
{
    public static class StompMessageConstants
    {
        public const string CONNECTFRAME = "CONNECT\n" + "\n\n\0";

        public const string CONNECTFRAMEWITHCREDENTIALS = "CONNECT\n" + "login:" + "{0}" + "\npasscode:"+ "{1}" + "\n\n\0";

        public const string SUBSCRIBETOPICFRAME = "SUBSCRIBE\n" + "destination:/topic/" + "{0}" + "\ncontent-type:application/octet-stream" + "\n\n\0";
        public const string SUBSCRIBEQUEUEFRAME = "SUBSCRIBE\n" + "destination:/queue/" + "{0}" + "\ncontent-type:application/octet-stream" + "\n\n\0";

        public const string ERRORHEADER = "ERROR";

        public const string CONTENTLENGTHHEADER = "content-length";
        public const string TYPEHEADER = "type";
        public const string CONTENTTYPECUSTOMHEADER = "contenttype";

        public const string HEADERKEYVALUESEPARATOR = ":";

       //It marks the end of actual message content
        public const string ENDOFMESSAGECONTENT = "\0\n";

        //It marks the end of complete message header
        public const string ENDOFMESSAGEHEADERS = "\n\n";

        //It marks end of a single message header in header list
        public const string ENDOFHEADER = "\n";

        //No of minimum bytes a message should contain to be processed.
        public const int TENTATIVEMESSAGEHEADERREADABLELENGTH = 250;
        public const int TENTATIVEMINIMUMMESSAGEHEADERLENGTH = 50;

        public const int READINCHUNKSLENGTH = 5000; //bytes        
    }
    
    public static class AppConfigConstants
    {
        public const string TOPICDESTINATIONTYPE = "topic";
        public const string QUEUEDESTINATIONTYPE = "queue";

        public const string DEFAULTMESSAGECONTENTTYPE = "dat";
        public const string TEXTMESSAGECONTENTTYPE = "txt";

        public const string TEMPDIRECTORYNAME = "GlassfishSubscriber";

        public const string TIMESTAMPFORMAT = "yyyyMMddHHmmssfff";

        public const string DESTINATIONPROPERTYHOST = "host";
        public const string DESTINATIONPROPERTYPORT = "port";
        public const string DESTINATIONPROPERTYUSERNAME = "username";
        public const string DESTINATIONPROPERTYPASSWORD = "password";
        public const string DESTINATIONPROPERTYTYPE = "type";
        public const string DESTINATIONPROPERTYNAME = "name";

        public const string DESTINATION = "Destination";
        public const string DESTINATIONS = "Destinations";
        
        public const string STOMPCONNECTCONFIGURATIONSECTION = "StompConnectConfiguration";
        public const string GLASSFISHSUBSCRIBERCONFIGURATIONSECTION = "GlassfishSubscriberConfiguration";

        public const string STORAGECONFIGURATION = "StorageConfiguration";

        public const string STORAGECONFIGPROPERTYSTORAGELOCATION = "storagelocation";
        public const string STORAGECONFIGPROPERTYBASEFILENAME = "basefilename";
        public const string STORAGECONFIGPROPERTYSAVEONDISK = "saveondisk";

        public const string DEFAULTBASEFILENAME = "GlassFish_Message_";

        public const string FILENAMESEPARATOR = "_";
    }

    public static class DBConstants
    {
        public const string CONNECTIONSTRINGKEY = "DBconnectionstring";

        public const string SAVEVERBATIAMSTOREDPROC = "SaveVerbatiamFile";

        public const string SAVEVERBATIAMSTOREDPROCPARAMETER = "data";
    }
}
