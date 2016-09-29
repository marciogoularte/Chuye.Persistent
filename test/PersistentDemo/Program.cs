using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.Linq;
using NLog;
using Chuye.Persistent.NHibernate;
using Chuye.Persistent.NHibernate.Impl;
using PersistentDemo.Model;

namespace PersistentDemo {
    class Program {
        static ILogger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {
            ReferenceMapNoUsingTest();
            ReferenceMapTest();
            ReferenceAsIdTest();
            ParentChildTest();

            Save_entity_new_and_exists();
            Retrive_via_primaryKey_medium_scale();

            //'NHibernate.Cfg.Environment.ShowSql' should set to false 
            //Insert_with_nhibernate();
            //Insert_with_petapoco();
        }

        /* 事务控制, 非事务行为, benchmark
        ### 事务控制
        1. NHibernateUnitOfWork.Dispose() 中会对未提交的事务进行回滚, 
           <add key="NHibernate:alwaysCommit" value="true" /> 可以修改此行为为提交；
        2. 存在事务控制即 NHibernateUnitOfWork.Begin() 时，应显式调用 NHibernateUnitOfWork.Commit() 
           或者使用 using(NHibernateUnitOfWork.Begin()) 确保事务得到提交；

        ### 非事务行为
        1. NHibernateUnitOfWork.Dispose() 调用 ISession.Dispose()，导致变更处理未知状态
        2. 如果期望保存变更，应显式调用 NHibernateUnitOfWork.Flush()
        3. <add key="NHibernate:alwaysCommit" value="true" /> 将修改 NHibernateUnitOfWork.Dispose() 的行为添加 Flush 操作

        ### benchmark
        1. auto_increment + transaction
            [PetaPoco.PrimaryKey("Id", AutoIncrement = true)]
            Id(x => x.Id);

            db.BeginTransaction();
            //Id = ++maxId,
            db.CompleteTransaction();

            context.Begin();
            ...
            //Id = ++maxId,
            //context.Flush();
            context.Commit();

            Nhibernate insert 1000, take 00:00:01.1058291 sec., 904.30/sec.
            PetaPoco insert 1000, take 00:00:00.4857616 sec., 2058.62/sec.
        2. auto_increment + no transaction
            [PetaPoco.PrimaryKey("Id", AutoIncrement = false)]
            Id(x => x.Id).GeneratedBy.Assigned();

            //db.BeginTransaction();
            //db.CompleteTransaction();

            //context.Begin();
            ...
            context.Flush();
            //context.Commit();

            Nhibernate insert 1000, take 00:00:02.8527109 sec., 350.54/sec.
            PetaPoco insert 1000, take 00:00:02.0649088 sec., 484.28/sec.
        3. not auto_increment + transaction
            [PetaPoco.PrimaryKey("Id", AutoIncrement = false)]
            Id(x => x.Id).GeneratedBy.Assigned();


            db.BeginTransaction();
            ...
            Id = ++maxId,
            db.CompleteTransaction();

            context.Begin();
            ...
            Id = ++maxId,
            //context.Flush();
            context.Commit();

            Nhibernate insert 1000, take 00:00:00.7060245 sec., 1416.38/sec.
            PetaPoco insert 1000, take 00:00:00.4870174 sec., 2053.31/sec.

        4. not auto_increment + no transaction
            Nhibernate insert 1000, take 00:00:00.8406925 sec., 1189.50/sec.
            PetaPoco insert 1000, take 00:00:01.4457931 sec., 691.66/sec.
        */

        #region mappings and UnitOfWork with transaction

        //从属账户思路?
        private static void ParentChildTest() {
            //LogManager.Configuration.AllTargets[0].
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            using (var context = new DbContext())
            using (var uow = new NHibernateUnitOfWork(context)) {
                //uow.Begin();
                var session = uow.OpenSession();
                var parent = new Node {
                    Name = "Parent",
                };
                logger.Trace("Session.Save()");
                session.Save(parent);
                logger.Trace("NHibernateUnitOfWork.Commit()");
                uow.Commit();

                var child = new Node {
                    Name = "Child",
                    Parent = parent,
                };
                logger.Trace("Session.Save()");
                session.Save(child);
                //logger.Trace("NHibernateUnitOfWork.Commit()");
                //uow.Commit();

                logger.Trace("NHibernateUnitOfWork.Dispose()");
                logger.Trace("NHibernateContext.Dispose()");
            }
        }

        //http://stackoverflow.com/questions/3634710/using-a-reference-as-id-in-fluentnhibernate
        private static void ReferenceAsIdTest() {
            using (var context = new DbContext())
            using (var uow = new NHibernateUnitOfWork(context)) {
                var session = uow.OpenSession();
                var user = new User {
                    Id = 13,
                    Name = "Rattz"
                };
                session.Save(user);
                var account = new Account {
                    User = user,
                    Balance = 100
                };
                session.Save(account);

                var drivingLicense = new DrivingLicense {
                    User = user,
                    CreateAt = DateTime.UtcNow
                };
                session.Save(drivingLicense);

                uow.Flush();
                //1. NHibernate:alwaysCommit=false，不显式调用 flush 可能导致变更保存否未知；
                //2. NHibernate:alwaysCommit=true，flush 将在 dispose 时被动调用
            }
        }

        private static void ReferenceMapTest() {
            var context = new DbContext();

            using (var uow = new NHibernateUnitOfWork(context))
            using (uow.Begin()) {
                var session = uow.OpenSession();
                var owner = new Owner {
                    Id = 1,
                };
                session.Save(owner);

                var foo = new FooItem() {
                    Id = 2,
                    Title = "title",
                    Owner = owner,
                };
                var bar = new BarItem() {
                    Id = 3,
                    Remark = "remark",
                    Owner = owner,
                };

                owner.FooItem = foo;
                owner.BarItem = bar;
                session.Save(owner);
            }

            //using (var uow = new NHibernateUnitOfWork(context))
            //using (uow.Begin()) {
            //    var session = uow.OpenSession();
            //    var query = session.Query<FooItem>().Where(x => x.Id < 0).Sum(x => (int?)x.Id) ?? 0;
            //}
        }

        private static void ReferenceMapNoUsingTest() {
            var context = new DbContext();

            using (var uow = new NHibernateUnitOfWork(context)) {
                uow.Begin();
                var session = uow.OpenSession();
                var owner = new Owner {
                    Id = 1,
                };
                session.Save(owner);

                var foo = new FooItem() {
                    Id = 2,
                    Title = "title",
                    Owner = owner,
                };
                var bar = new BarItem() {
                    Id = 3,
                    Remark = "remark",
                    Owner = owner,
                };

                owner.FooItem = foo;
                owner.BarItem = bar;
                session.Save(owner);

                //uow.Flush(); // 在事务控制中，flush 不能保证变更被保存；
                //uow.Commit();
                //1. NHibernate:alwaysCommit=false，不显式调用 commit, 无论是否 flush，变更都被回滚；
                //2. NHibernate:alwaysCommit=true，commit 将在 dispose 时被动调用
            }

            //using (var uow = new NHibernateUnitOfWork(context))
            //using (uow.Begin()) {
            //    var session = uow.OpenSession();
            //    var query = session.Query<FooItem>().Where(x => x.Id < 0).Sum(x => (int?)x.Id) ?? 0;
            //}
        }

        #endregion

        #region repository

        private static void Retrive_via_primaryKey_medium_scale() {
            using (var db = new DbContext())
            using (var context = new NHibernateUnitOfWork(db)) {
                var repo = new NHibernateRepository<Job>(context);
                var allKeys = repo.All.Select(x => x.Job_id).ToArray();
                Console.WriteLine("Keys.Length={0}", allKeys.Length);
                var allItems = repo.Retrive(x => x.Job_id, allKeys).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
                allItems = repo.All.Where(r => allKeys.Contains(r.Job_id)).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
            }
        }

        private static void Save_entity_new_and_exists() {
            var theOne = new Job {
                Job_id = (short)Guid.NewGuid().GetHashCode(),
                Job_desc = Guid.NewGuid().ToString(),
            };

            using (var db = new DbContext())
            using (var context = new NHibernateUnitOfWork(db)) {
                var repo = new NHibernateRepository<Job>(context);
                repo.Save(theOne);

                theOne.Min_lvl++;
                repo.Update(theOne);
                context.Flush();
            }

            using (var db = new DbContext())
            using (var context = new NHibernateUnitOfWork(db)) {
                var repo = new NHibernateRepository<Job>(context);
                theOne.Max_lvl--;
                repo.Save(theOne);
                context.Flush();
            }
        }

        #endregion

        #region benchmark
        private static void Insert_with_petapoco(Int32 count = 1000) {
            var stopwatch = Stopwatch.StartNew();

            using (var db = new PetaPoco.Database("test")) {
                //db.BeginTransaction();
                var maxId = db.ExecuteScalar<Int32>("select ifnull( max(id),0) from person;");
                stopwatch.Start();
                for (int i = 0; i < count; i++) {
                    var person = new Person {
                        Id = ++maxId,
                        Name = Guid.NewGuid().ToString().Substring(0, 8),
                        Address = Guid.NewGuid().ToString(),
                        Birth = DateTime.Now,
                        Job_id = Math.Abs(Guid.NewGuid().GetHashCode() % 100)
                    };
                    db.Insert(person);
                }
                //db.CompleteTransaction();
            }
            stopwatch.Stop();
            Console.WriteLine("PetaPoco insert {0}, take {1} sec., {2:f2}/sec.",
                count, stopwatch.Elapsed, count / stopwatch.Elapsed.TotalSeconds);
        }

        private static void Insert_with_nhibernate(Int32 count = 1000) {
            var stopwatch = Stopwatch.StartNew();

            using (var db = new DbContext())
            using (var context = new NHibernateUnitOfWork(db)) {
                stopwatch.Restart(); // delay record for SessionFactory construct
                                     //context.Begin();
                var repo = new NHibernateRepository<Person>(context);

                var maxId = context.OpenSession().CreateCriteria<Person>()
                    .SetProjection(Projections.Max<Person>(x => x.Id))
                    .UniqueResult<Int32>();

                for (int i = 0; i < count; i++) {
                    var person = new Person {
                        Id = ++maxId,
                        Name = Guid.NewGuid().ToString().Substring(0, 8),
                        Address = Guid.NewGuid().ToString(),
                        Birth = DateTime.Now,
                        Job_id = Math.Abs(Guid.NewGuid().GetHashCode() % 100)
                    };
                    repo.Create(person);
                }
                context.Flush();
                //context.Commit();
            }
            stopwatch.Stop();
            Console.WriteLine("Nhibernate insert {0}, take {1} sec., {2:f2}/sec.",
                count, stopwatch.Elapsed, count / stopwatch.Elapsed.TotalSeconds);
        }

        #endregion
    }
}
