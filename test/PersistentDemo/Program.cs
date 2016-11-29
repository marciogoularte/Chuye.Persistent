using System;
using System.Diagnostics;
using NLog;

namespace PersistentDemo {
    class Program {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        /*static void TransactionManage() {
            var config = new NHibernateDbConfig {
                ModificationStragety = ModificationStragety.Discard,
                TransactionDemand =  TransactionDemand.Manual,
                TransactionTime = TransactionTime.SessionStarted
            };

            var context = new DbContext();
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                //TransactionTime.Immediately, use Begin(), get true
                Console.WriteLine(session.Transaction.IsActive);

                uow.Begin();
                //TransactionTime.Immediately, use Begin(), get true
                Console.WriteLine(session.Transaction.IsActive);

                session.CreateSQLQuery("select now()").ExecuteUpdate();
                Console.WriteLine(session.Transaction.IsActive);
            }
            Console.WriteLine();

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                var session = uow.OpenSession();
                //TransactionTime.Immediately, use Begin(), get true
                Console.WriteLine(session.Transaction.IsActive);

                uow.Commit();
                Console.WriteLine(session.Transaction.IsActive);
            }
            Console.WriteLine();

            config = new NHibernateDbConfig {
                ModificationStragety = ModificationStragety.Submit,
                TransactionDemand = TransactionDemand.Manual,
                TransactionTime = TransactionTime.Immediately
            };
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.Begin();
                var session = uow.OpenSession();
                //TransactionTime.Immediately, use Begin(), get true
                Console.WriteLine(session.Transaction.IsActive);
            }
            Console.WriteLine();

            config = new NHibernateDbConfig {
                ModificationStragety = ModificationStragety.Discard,
                TransactionDemand = TransactionDemand.Essential,
                TransactionTime = TransactionTime.SessionStarted
            };

            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                //TransactionDemand.Essential, get true
                Console.WriteLine(session.Transaction.IsActive);

                uow.Commit();
                //Commit(), get true though 
                Console.WriteLine(session.Transaction.IsActive);
            }
            Console.WriteLine();

            config = new NHibernateDbConfig {
                ModificationStragety = ModificationStragety.Submit,
                TransactionDemand = TransactionDemand.Essential,
                TransactionTime = TransactionTime.SessionStarted
            };
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                //TransactionTime.Immediately, use Begin(), get true
                Console.WriteLine(session.Transaction.IsActive);
            }
            Console.WriteLine();
        }   */
    }
}
