using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NHibernate;
using PersistentDemo;
using Xunit;

namespace Chuye.Persistent.NHibernate.Test {
    public class NHibernateDbConfigTest {
        [Fact]
        public void TransactionRequire_manual() {
            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Manual
                }
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

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var dispose = uow.Begin();
                var session = uow.OpenSession();

                dispose.Dispose();
                Assert.False(session.Transaction.IsActive);
            }
        }

        [Fact]
        public void TransactionRequire_essential() {
            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Essential
                }
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

        [Fact]
        public void TransactionTime_Lazy() {
            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Time = TransactionTime.Lazy
                }
            };
            var context = new DbContext();

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                Assert.Equal(uow.Count, 0);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                Assert.Equal(uow.Count, 0);
            }

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();

                var session = uow.OpenSession();
                Assert.Equal(uow.Count, 1);
                Assert.True(session.Transaction.IsActive);
            }
        }
    }
}
