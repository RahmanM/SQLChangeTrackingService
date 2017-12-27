using Nerdle.AutoConfig;
using Sql.ChangeTracking.Data;
using System;
using System.Collections.Generic;

namespace SqlChangeTrackingProducerConsumer.DI
{
    internal class DatabaseHelper : IDatabaseHelper
    {

        public ChangeTrackingAppSettings ChangeTrackingAppSettings { get; set; }
        public DatabaseHelper()
        {
            // This cool util, serializes the app.config configuration section to a class or interface
            ChangeTrackingAppSettings = AutoConfig.Map<ChangeTrackingAppSettings>();
        }

        public List<UspTableVersionChangeTrackingReturnModel> GetData()
        {
            using (var context = new SQLChangeTrackingTest("SQLChangeTrackingTest"))
            {
                return context.UspTableVersionChangeTracking(ChangeTrackingAppSettings.ChangeTrackingVersionToStart);
            }
        }
    }
}