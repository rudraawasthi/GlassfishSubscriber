﻿using System.ServiceProcess;

namespace GlassfishSubscriber
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new GlassfishSubscriberService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
