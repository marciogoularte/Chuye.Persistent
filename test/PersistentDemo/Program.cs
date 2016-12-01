using System;
using System.Diagnostics;
using NLog;
using Chuye.Persistent.NHibernate;
using PersistentDemo.Models;
using System.Configuration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

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

            //var context = new DbContext();
            var context = new NHibernateDbContext(config).Setup(() =>
                Fluently.Configure()
                    .Database(MySQLConfiguration.Standard.ConnectionString(ConfigurationManager.ConnectionStrings["test"].ConnectionString))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DbContext>())
                    .BuildConfiguration()
                    .SetProperty(NHibernate.Cfg.Environment.ShowSql, Boolean.TrueString));

            Console.WriteLine("{0} Prepare data", DateTime.Now);
            using (var uow = new NHibernateUnitOfWork(context))
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
            Console.WriteLine("{0} Get reference without trans", DateTime.Now);
            using (var uow = new NHibernateUnitOfWork(context, config)) {
                var session = uow.OpenSession();
                var desktop = session.Get<Drawer>(100);
            }
            Console.WriteLine();

            Console.WriteLine("{0} Get reference with trans commmit", DateTime.Now);
            using (var uow = new NHibernateUnitOfWork(context, config))
            using (uow.Begin()) {
                var session = uow.OpenSession();
                var desktop = session.Get<Drawer>(100);
            }
            Console.WriteLine();
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
