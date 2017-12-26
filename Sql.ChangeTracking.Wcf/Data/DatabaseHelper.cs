using Sql.ChangeTracking.Data;
using System;
using System.Collections.Generic;

namespace ServiceTopShelf.DI
{
    internal class DatabaseHelper : IDatabaseHelper
    {
        public List<UspTableVersionChangeTrackingReturnModel> GetData()
        {
            using (var context = new SQLChangeTrackingTest("SQLChangeTrackingTest"))
            {
                return context.UspTableVersionChangeTracking(null);
            }
        }
    }
}