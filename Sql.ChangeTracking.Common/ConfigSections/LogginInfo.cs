using Serilog;
using System.IO;
using System;

namespace SqlChangeTrackingProducerConsumer
{

    /// <summary>
    /// Configuration section specific to loggin used by Nerdle.AutoConfig
    /// </summary>
    public class LogginInfo
    {
        public string LogFile { get; set; }
        public string LogFolder { get; set; }
    }

}
