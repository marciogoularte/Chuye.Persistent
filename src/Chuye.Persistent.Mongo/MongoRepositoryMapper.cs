﻿using System;
using MongoDB.Driver;

namespace Chuye.Persistent.Mongo {

    public interface IMongoRepositoryMapper {
        String Map<TEntry>();
    }

    public class MongoRepositoryMapper : IMongoRepositoryMapper {
        public String Map<TEntry>() {
            var entryType = typeof(TEntry);
            return entryType.Name;
        }
    }
}
