using System.ServiceModel;

namespace Sql.ChangeTracking.Common
{

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IEventNotificationCallback))]
    public interface IChangeTrackingSubscriptions
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string id, string tableName);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string id, string tableName);

        [OperationContract(IsOneWay = true)] 
        void TableChanged(string tableName);
    }
}
