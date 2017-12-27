using Sql.ChangeTracking.Data;
using System;
using System.Collections.Generic;

namespace SqlChangeTrackingProducerConsumer.DI
{
    internal class DatabaseHelper : IDatabaseHelper
    {
        public List<UspTableVersionChangeTrackingReturnModel> GetData()
        {
            using (var context = new SQLChangeTrackingTest("SQLChangeTrackingTest"))
            {
                return context.UspTableVersionChangeTracking(1);
            }
        }
    }
}