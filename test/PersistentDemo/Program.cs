using System;
using System.Diagnostics;
using NLog;
using Chuye.Persistent.NHibernate;
using PersistentDemo.Models;

namespace PersistentDemo {
    class Program {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            Debug.Listeners.Clear();
            //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Manual,
                    Time = TransactionTime.Lazy
                },
                SaveUncommitted = false
            };
            var context = new DbContext();
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                uow.OpenSession().CreateSQLQuery("Delete from Drawer").ExecuteUpdate();
            }

            var repeat = 1000;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < repeat; i++) {
                using (var uow = new NHibernateUnitOfWork(context, config)) {
                    uow.Begin();
                    var drawer = new Drawer {
                        Id = i,
                        Name = Guid.NewGuid().ToString("n")
                    };
                    uow.OpenSession().Save(drawer);
                    uow.Commit();
                }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            Console.WriteLine();

            watch.Restart();
            for (int i = repeat; i < repeat * 2; i++) {
                using (var uow = new NHibernateUnitOfWork(context, config)) {
                    uow.Begin();
                    var drawer = new Drawer {
                        Id = i,
                        Name = Guid.NewGuid().ToString("n")
                    };
                    uow.OpenSession().Save(drawer);
                    uow.Commit();
                    uow.Dispose();
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            Console.WriteLine();

            config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Essential,
                    Time = TransactionTime.Lazy
                },
                SaveUncommitted = true
            };
            watch.Restart();
            for (int i = repeat * 2; i < repeat * 3; i++) {
                using (var uow = new NHibernateUnitOfWork(context, config)) {
                    var drawer = new Drawer {
                        Id = i,
                        Name = Guid.NewGuid().ToString("n")
                    };
                    uow.OpenSession().Save(drawer);
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }
}
