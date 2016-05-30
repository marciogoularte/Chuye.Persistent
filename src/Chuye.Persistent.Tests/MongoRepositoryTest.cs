using System;
using System.Linq;
using System.Collections.Generic;
using Chuye.Persistent.NHibernate;
using Xunit;
using Chuye.Persistent.Mongo;
using Chuye.Persistent.Tests.Mongo;
using MongoDB.Bson;

namespace Chuye.Persistent.Tests {
    public class MongoRepositoryTest {
        private const Byte Max_lvl_HasValue = (Byte)100;
        private readonly PubsContext _context;

        public MongoRepositoryTest() {
            _context = new PubsContext();
        }

        [Fact]
        public void Retrive_via_primaryKey() {
            var repo = new MongoRepository<Job>(_context);
            var theFirstOne = repo.Retrive(ObjectId.Parse("574c0b01ca3c5fa8b8cfa0c8"));
        }

        [Fact]
        public void Retrive_via_field() {
            var repo = new MongoRepository<Job>(_context);
            var theSpecials = repo.Retrive("Max_lvl", (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_expression() {
            var repo = new MongoRepository<Job>(_context);
            var theSpecials = repo.Retrive(j => j.Max_lvl, (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_queryable() {
            var repo = new MongoRepository<Job>(_context);
            var query = repo.All.Where(r => r.Max_lvl == Max_lvl_HasValue);
            var theSpecials = repo.Fetch(_ => query);
            Assert.NotEmpty(theSpecials);
        }
    }
}
