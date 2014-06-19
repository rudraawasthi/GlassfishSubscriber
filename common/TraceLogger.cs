using GlassfishSubscriber.resource;
using System;
using System.Diagnostics;
using System.Threading;

namespace GlassfishSubscriber
{
    internal static class TraceLogger
    {
        public static void Log(string message, params object[] parameterList)
        {
            lock (_lockObject)
            {
                Trace.TraceInformation(NonLocalizableResource.LogTimeMessage, DateTime.UtcNow.ToString(), DateTime.Now.ToString());
                Trace.TraceInformation(NonLocalizableResource.LogThreadInfoMessage, Thread.CurrentThread.ManagedThreadId);
                Trace.TraceInformation(message, parameterList);
                Trace.WriteLine(string.Empty);
            }
        }

        public static void Log(Exception exception)
        {
            lock (_lockObject)
            {
                if (null == exception)
                {
                    Trace.WriteLine(string.Empty);
                    return;
                }
                Trace.TraceError(NonLocalizableResource.ErrorLogTimeMessage, DateTime.UtcNow.ToString(), DateTime.Now.ToString());
                Trace.TraceError(NonLocalizableResource.LogThreadInfoMessage,Thread.CurrentThread.ManagedThreadId);
                Trace.TraceError(exception.ToString());

                //Recursively log all the inner exceptions
                Log(exception.InnerException);
            }
        }

        public static void LogWarning(string message, params object[] parameterList)
        {
            lock (_lockObject)
            {
                Trace.TraceWarning(NonLocalizableResource.WarningLogTimeMessage, DateTime.UtcNow.ToString(), DateTime.Now.ToString());
                Trace.TraceWarning(NonLocalizableResource.LogThreadInfoMessage, Thread.CurrentThread.ManagedThreadId);
                Trace.TraceWarning(message, parameterList);
                Trace.WriteLine(string.Empty);
            }
        }

        private static readonly object _lockObject = new object();
    }
}
