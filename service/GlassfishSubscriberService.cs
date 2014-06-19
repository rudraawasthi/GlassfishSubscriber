using GlassfishSubscriber.resource;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace GlassfishSubscriber
{
    public partial class GlassfishSubscriberService : ServiceBase
    {
        public GlassfishSubscriberService()
        {
            InitializeComponent();
            SetEventLog();
        }

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(10000); //for unit testing

            eventLogGlassfishService.WriteEntry(NonLocalizableResource.ServiceStarting, EventLogEntryType.Information);

            try
            {
                Task.Factory.StartNew(() => Subscriber.Subscribe());

                eventLogGlassfishService.WriteEntry(NonLocalizableResource.ServiceStarted, EventLogEntryType.Information);
                TraceLogger.Log(NonLocalizableResource.ServiceStarted);
            }
            catch (Exception e)
            {
                string errorMessage = string.Concat(NonLocalizableResource.ServiceStartError, e.Message);

                eventLogGlassfishService.WriteEntry(errorMessage, EventLogEntryType.Error);
                TraceLogger.Log(errorMessage);
            }
        }

        protected override void OnStop()
        {
            eventLogGlassfishService.WriteEntry(NonLocalizableResource.ServiceStopping, EventLogEntryType.Information);

            try
            {
                Subscriber.Unsubscribe();

                eventLogGlassfishService.WriteEntry(NonLocalizableResource.ServiceStopped, EventLogEntryType.Information);
                TraceLogger.Log(NonLocalizableResource.ServiceStopped);
            }
            catch (Exception e)
            {
                string errorMessage = string.Concat(NonLocalizableResource.ServiceStopError, e.Message);

                eventLogGlassfishService.WriteEntry(errorMessage, EventLogEntryType.Error);
                TraceLogger.Log(errorMessage);
            }
        }

        private void SetEventLog()
        {
            bool sourceExists;
            try
            {
                sourceExists = System.Diagnostics.EventLog.SourceExists(NonLocalizableResource.EventlogSource);
            }
            catch (Exception e)
            {
                string errorMessage = string.Concat(NonLocalizableResource.EventLogSourceCheckError, e.Message);

                eventLogGlassfishService.WriteEntry(errorMessage, EventLogEntryType.Information);
                sourceExists = false;
            }

            if (!sourceExists)
            {
                System.Diagnostics.EventLog.CreateEventSource(NonLocalizableResource.EventlogSource, NonLocalizableResource.EventlogName);
                return;
            }

            eventLogGlassfishService.Source = NonLocalizableResource.EventlogSource;
            eventLogGlassfishService.Log = NonLocalizableResource.EventlogName;
        }

        private SubscriberFacade Subscriber
        {
            get
            {
                if (null == _glassfishSubscriber)
                    _glassfishSubscriber = new SubscriberFacade();
                return _glassfishSubscriber;
            }
        }

        #region PrivateMembers

        private SubscriberFacade _glassfishSubscriber;

        #endregion
    }
}
