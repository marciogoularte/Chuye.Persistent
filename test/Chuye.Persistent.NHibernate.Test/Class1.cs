using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NHibernate;
using PersistentDemo;
using Xunit;

namespace Chuye.Persistent.NHibernate.Test {
    public class Class1 {
        [Fact]
        public void Demand_manual() {
            var config = new NHibernateDbConfig {
                TransactionDemand = TransactionDemand.Manual,
            };
            var context = new DbContext();

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                Assert.False(session.Transaction.IsActive);

                uow.Begin();
                Assert.True(session.Transaction.IsActive);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                var session = uow.OpenSession();
                Assert.True(session.Transaction.IsActive);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                var session = uow.OpenSession();

                uow.Commit();
                Assert.False(session.Transaction.IsActive);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                var session = uow.OpenSession();

                uow.Rollback();
                Assert.False(session.Transaction.IsActive);
            }
        }

        [Fact]
        public void Demand_essential() {
            var config = new NHibernateDbConfig {
                TransactionDemand = TransactionDemand.Essential,
            };
            var context = new DbContext();

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                Assert.True(session.Transaction.IsActive);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                uow.Commit();
                Assert.True(session.Transaction.IsActive);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                uow.Rollback();
                Assert.True(session.Transaction.IsActive);
            }
        }
    }
}
