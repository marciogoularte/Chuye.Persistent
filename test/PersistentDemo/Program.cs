using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Chuye.Persistent.NHibernate;
using Chuye.Persistent.PetaPoco;
using NHibernate.Linq;
using NLog;
using PersistentDemo.Models;

namespace PersistentDemo {
    class Program {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            RepositoryTest();

            if (Debugger.IsAttached) {
                Console.WriteLine("Press <ENTER> to exit");
                Console.ReadLine();
            }
        }

        static void RepositoryTest() {
            var context = new DbContext();
            using (var uow = new NHibernateUnitOfWork(context))
            using (uow.Begin()) {
                var repo = new NHibernateRepository<Desktop>(uow);
                var desktop = new Desktop {
                    Id = 10,
                    Title = "title#1",
                    Status = GeneralType.Classified,
                };
                desktop.Drawers.Add(new Drawer {
                    Id = 100,
                    Name = "name#1",
                    Desktop = desktop,
                });
                desktop.Drawers.Add(new Drawer {
                    Id = 101,
                    Name = "name#2",
                    Desktop = desktop,
                });
                repo.Save(desktop);
            }

            using (var uow = new NHibernateUnitOfWork(context, new NHibernateDbConfig { SaveUncommitted = true }))
            /*using (uow.Begin()) */{
                var repo1 = new NHibernateRepository<Drawer>(uow);
                repo1.Save(new Drawer {
                    Id = 200,
                    Name = "name#3",
                });

                var drawers = repo1.FindByKeys(100, 101).ToArray();
                repo1.Delete(drawers[0]);

                var repo2 = new NHibernateRepository<Desktop>(uow);
                var desktop = repo2.FindById(10);
                desktop.Title = "title#1 modified";
                desktop.Status = GeneralType.Specific;
                repo2.Save(desktop);

            }
        }

        static void HybridTransTest() {
            var context1 = new DbContext();
            using (var uow1 = new NHibernateUnitOfWork(context1)) {
                using (uow1.Begin()) {
                    var uow2 = new PetaPocoUnitOfWork(uow1.OpenSession().Connection);
                    uow2.Database.Execute("update Desktop set DrawerId = 100 where Id = 9");
                }
            }
        }

        static void PetaPocoTransTest() {
            var context = new PetaPocoUnitOfWork("test");
            //using (context) {
            context.Begin();
            //context.Begin().Dispose();
            //context.Begin();
            //context.Commit();
            //context.Begin();
            //context.Rollback();
            //context.Commit();
            //}
        }

        static void ReferenceLoadTest() {
            var config = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Manual,
                    Time = TransactionTime.Lazy
                },
                SaveUncommitted = false
            };

            var context = new DbContext();
            Console.WriteLine("{0:HH:mm:ss.fff} Prepare data", DateTime.Now);
            using (var uow = new NHibernateUnitOfWork(context))
            using (uow.Begin()) {
                var session = uow.OpenSession();
                session.CreateSQLQuery("Delete from Drawer").ExecuteUpdate();
                session.CreateSQLQuery("Delete from Desktop").ExecuteUpdate();

                for (int i = 0; i < 100; i++) {
                    var desktop = new Desktop {
                        Id = i,
                    };
                    session.Save(desktop);

                    session.Save(new Drawer {
                        Id = 10000 + i,
                        Desktop = desktop,
                    });
                }
            }
            Console.WriteLine();

            /*The N+1 Problem under Transaction*/
            Console.WriteLine("{0:HH:mm:ss.fff} Query lists", DateTime.Now);
            using (var uow = new NHibernateUnitOfWork(context, config))
            /*using (uow.Begin())*/ {
                var session = uow.OpenSession();
                var drawers = session.Query<Drawer>().ToList();
                //Console.WriteLine("{0:HH:mm:ss.fff} Access props", DateTime.Now);
                //drawers.Select(x => x.Desktop.Id).ToArray();
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
