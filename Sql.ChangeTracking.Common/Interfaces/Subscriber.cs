using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Sql.ChangeTracking.Common
{

    public class Subscriber
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string TableInterested { get; set; }
    }
}
