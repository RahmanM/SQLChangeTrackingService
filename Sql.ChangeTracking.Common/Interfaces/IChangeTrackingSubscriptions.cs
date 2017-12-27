using Serilog;
using Sql.ChangeTracking.Data;
using System.ServiceModel;

namespace Sql.ChangeTracking.Common
{
    // NB: It requires session to enable callback, and CallbackContract is important!!
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IEventNotificationCallback))]
    public interface IChangeTrackingSubscriptions
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string id, string tableName);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string id, string tableName);

        [OperationContract(IsOneWay = true)] 
        void TableChanged(UspTableVersionChangeTrackingReturnModel table);

        ILogger Logger { get; set; }
    }
}
