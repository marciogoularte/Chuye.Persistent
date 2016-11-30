using System;
using System.Diagnostics;
using NLog;
using Chuye.Persistent.NHibernate;
using PersistentDemo.Models;

namespace PersistentDemo {
    class Program {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            //Debug.Listeners.Clear();
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            ReferenceMapSaveTest();
        }

        private static void ReferenceMapSaveTest() {
            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Manual,
                    Time = TransactionTime.Lazy
                },
                SaveUncommitted = false
            };

            var context = new DbContext();

            Console.WriteLine("Prepare data");
            using (var uow = new NHibernateUnitOfWork(context, config))
            using (uow.Begin()) {
                var session = uow.OpenSession();
                session.CreateSQLQuery("Delete from Drawer").ExecuteUpdate();
                session.CreateSQLQuery("Delete from Desktop").ExecuteUpdate();

                var desktop = new Desktop {
                    Id = 100,
                };
                session.Save(desktop);

                session.Save(new Drawer {
                    Id = 100,
                    Desktop = desktop,
                });
                session.Save(new Drawer {
                    Id = 101,
                    Desktop = desktop,
                });
                session.Save(new Drawer {
                    Id = 102,
                    Desktop = desktop,
                });
            }
            Console.WriteLine();

            //using (var uow = new NHibernateUnitOfWork(context, config)) {
            //    var session = uow.OpenSession();
            //    var desktop = session.Get<Desktop>(100);
            //}
            Console.WriteLine("Get reference without trans");
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                var desktop = session.Get<Drawer>(100);
            }
            Console.WriteLine();

            Console.WriteLine("Get reference with trans commmit");
            using (var uow = new NHibernateUnitOfWork(context, config))
            using (uow.Begin()) {
                var session = uow.OpenSession();
                var desktop = session.Get<Drawer>(100);
            }
        }

        static void TransactionStragetyTest() {
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
