using Sql.ChangeTracking.Data;
using System.Collections.Generic;

namespace SqlChangeTrackingProducerConsumer
{
    public interface IDatabaseHelper
    {
        List<UspTableVersionChangeTrackingReturnModel> GetData();
    }
}