using Sql.ChangeTracking.Data;
using System.Collections.Generic;

namespace ServiceTopShelf
{
    public interface IDatabaseHelper
    {
        List<UspTableVersionChangeTrackingReturnModel> GetData();
    }
}