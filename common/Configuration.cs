using GlassfishSubscriber.resource;
using System;
using System.Configuration;

namespace GlassfishSubscriber
{
    #region Subscriber service configuration

    public class GlassfishSubscriberConfiguration : ConfigurationSection
    {
        #region Constructor

        public GlassfishSubscriberConfiguration()
        {

        }

        public GlassfishSubscriberConfiguration(StorageConfiguration storageConfiguartion)
        {
            _config.StorageConfiguration = storageConfiguartion;
        }

        #endregion

        public static GlassfishSubscriberConfiguration SubscriberConfiguration
        {
            get { return _config; }

        }

        [ConfigurationProperty(AppConfigConstants.STORAGECONFIGURATION)]
        public StorageConfiguration StorageConfiguration
        {
            get
            {
                return base[AppConfigConstants.STORAGECONFIGURATION] as StorageConfiguration;
            }

            set
            {
                base[AppConfigConstants.STORAGECONFIGURATION] = value;
            }
        }


        #region Private Members

        private static GlassfishSubscriberConfiguration _config = ConfigurationManager.GetSection(AppConfigConstants.GLASSFISHSUBSCRIBERCONFIGURATIONSECTION) as GlassfishSubscriberConfiguration;

        #endregion
    }

    public class StorageConfiguration : ConfigurationElement
    {
        [ConfigurationProperty(AppConfigConstants.STORAGECONFIGPROPERTYSAVEONDISK)]
        public bool SaveOnDisk
        {
            get
            {
                return (bool)this[AppConfigConstants.STORAGECONFIGPROPERTYSAVEONDISK];
            }
            set
            {
                this[AppConfigConstants.STORAGECONFIGPROPERTYSAVEONDISK] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.STORAGECONFIGPROPERTYSTORAGELOCATION)]
        public string StorageLocation
        {
            get
            {
                return this[AppConfigConstants.STORAGECONFIGPROPERTYSTORAGELOCATION] as string;
            }
            set
            {
                this[AppConfigConstants.STORAGECONFIGPROPERTYSTORAGELOCATION] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.STORAGECONFIGPROPERTYBASEFILENAME, DefaultValue = AppConfigConstants.DEFAULTBASEFILENAME)]
        public string BaseFileName
        {
            get
            {
                return this[AppConfigConstants.STORAGECONFIGPROPERTYBASEFILENAME] as string;
            }
            set
            {
                this[AppConfigConstants.STORAGECONFIGPROPERTYBASEFILENAME] = value;
            }
        }
    }

    #endregion

    #region StompConnect configuration

    public class StompConnectConfiguration : ConfigurationSection
    {
        #region Constructor

        public StompConnectConfiguration()
        {
        }

        #endregion

        public static StompConnectConfiguration StompConfiguration
        {
            get { return _config; }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONS, IsDefaultCollection = true)]
        public DestinationCollection Destinations
        {
            get
            {
                return base[AppConfigConstants.DESTINATIONS] as DestinationCollection;
            }

            set
            {
                base[AppConfigConstants.DESTINATIONS] = value;
            }
        }

        #region Private Members

        private static StompConnectConfiguration _config = ConfigurationManager.GetSection(AppConfigConstants.STOMPCONNECTCONFIGURATIONSECTION) as StompConnectConfiguration;

        #endregion
    }

    #region Destination configuration classes

    public class DestinationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Destination();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Destination)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return AppConfigConstants.DESTINATION;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return !String.IsNullOrEmpty(elementName) && elementName == AppConfigConstants.DESTINATION;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public Destination this[int index]
        {
            get
            {
                return base.BaseGet(index) as Destination;
            }
        }
    }

    public class Destination : ConfigurationElement
    {
        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYHOST)]
        public string Host
        {
            get
            {
                return this[AppConfigConstants.DESTINATIONPROPERTYHOST] as string;
            }
            set
            {
                this[AppConfigConstants.DESTINATIONPROPERTYHOST] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYPORT)]
        public int Port
        {
            get
            {
                return (int)this[AppConfigConstants.DESTINATIONPROPERTYPORT];
            }
            set
            {
                this[AppConfigConstants.DESTINATIONPROPERTYPORT] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYPASSWORD)]
        public string Password
        {
            get
            {
                return this[AppConfigConstants.DESTINATIONPROPERTYPASSWORD] as string;
            }
            set
            {
                this[AppConfigConstants.DESTINATIONPROPERTYPASSWORD] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYUSERNAME)]
        public string UserName
        {
            get
            {
                return this[AppConfigConstants.DESTINATIONPROPERTYUSERNAME] as string;
            }
            set
            {
                this[AppConfigConstants.DESTINATIONPROPERTYUSERNAME] = value;
            }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYNAME)]
        public string Name
        {
            get { return base[AppConfigConstants.DESTINATIONPROPERTYNAME] as string; }
            set { base[AppConfigConstants.DESTINATIONPROPERTYNAME] = value; }
        }

        [ConfigurationProperty(AppConfigConstants.DESTINATIONPROPERTYTYPE)]
        public string Destinationtype
        {
            get { return base[AppConfigConstants.DESTINATIONPROPERTYTYPE] as string; }
            set { base[AppConfigConstants.DESTINATIONPROPERTYTYPE] = value; }
        }

    }

    #endregion

    #endregion

    public class DBConfiguration
    {
        static DBConfiguration()
        {

        }

        public DBConfiguration(ConnectionStringSettings connectionStringSettings)
        {
            _configurationString = connectionStringSettings;
        }

        public static string ConnectionString
        {
            get
            {
                if (null != _configurationString)
                    return _configurationString.ConnectionString;

                TraceLogger.Log(NonLocalizableResource.DBConnectionStringNotFound, DBConstants.CONNECTIONSTRINGKEY);
                return string.Empty;
            }
            set
            {
                _configurationString.ConnectionString = value;
            }
        }

        private static ConnectionStringSettings _configurationString = ConfigurationManager.ConnectionStrings[DBConstants.CONNECTIONSTRINGKEY];
    }

}
