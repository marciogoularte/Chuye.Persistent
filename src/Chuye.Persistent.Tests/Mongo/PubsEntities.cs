using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Chuye.Persistent.Mongo;

namespace Chuye.Persistent.Tests.Mongo {
    public class Job: IMongoAggregate {
        public ObjectId Id { get; set; }
        public virtual Int16 Job_id { get; set; }
        public virtual String Job_desc { get; set; }
        public virtual Byte Min_lvl { get; set; }
        public virtual Byte Max_lvl { get; set; }
    }
}
