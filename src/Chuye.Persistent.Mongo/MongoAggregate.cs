using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Chuye.Persistent.Mongo {

    public interface IMongoAggregate<TKey>  {
        TKey Id { get; set; }
    }
    public interface IMongoAggregate : IMongoAggregate<ObjectId> {
    }
}