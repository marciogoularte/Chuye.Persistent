using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Chuye.Persistent.Mongo {
    public interface IMongoId<TKey>  {
        TKey Id { get; set; }
    }
    public interface IMongoId : IMongoId<ObjectId> {
    }
}