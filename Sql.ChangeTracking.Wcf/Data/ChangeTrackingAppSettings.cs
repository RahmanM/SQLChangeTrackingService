using System;

namespace SqlChangeTrackingProducerConsumer.DI
{
    internal class ChangeTrackingAppSettings
    {
        public int? ChangeTrackingVersionToStart { get; set; }
        public int DegreeOfParallelism { get; set; }
        public int PollingFrequencyMilliSeconds { get; set; }
    }
}