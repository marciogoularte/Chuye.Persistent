using System;
using System.Linq;
using System.Collections.Generic;
using Chuye.Persistent.NHibernate;
using Xunit;

namespace Chuye.Persistent.Tests {
    public class NHibernateRepositoryTest {
        private const Byte Max_lvl_HasValue = (Byte)100;
        private readonly PubsContext _context;

        public NHibernateRepositoryTest() {
            _context = new PubsContext();
        }

        [Fact]
        public void Retrive_via_primaryKey() {
            var repo = new NHibernateRepository<Job>(_context);
            var theFirstOne = repo.Retrive((Int16)1);
        }

        [Fact]
        public void Retrive_via_field() {
            var repo = new NHibernateRepository<Job>(_context);
            var theSpecials = repo.Retrive("Max_lvl", (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_expression() {
            var repo = new NHibernateRepository<Job>(_context);
            var theSpecials = repo.Retrive(j => j.Max_lvl, (Byte)Max_lvl_HasValue);
            Assert.NotEmpty(theSpecials);
        }

        [Fact]
        public void Retrive_via_queryable() {
            var repo = new NHibernateRepository<Job>(_context);
            var query = repo.All.Where(r => r.Max_lvl == Max_lvl_HasValue);
            var theSpecials = repo.Fetch(_ => query);
            Assert.NotEmpty(theSpecials);
        }
    }
}
