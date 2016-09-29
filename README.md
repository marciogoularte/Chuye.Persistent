## Usage

1. Add dependency using nuget  
```bash
install-package FluentNHibernate
install-package Chuye.Persistent
```


2. Add mapping and poco  
```c
    class Node {
        public virtual int Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Node Parent { get; set; }
    }

    class NodeMap : ClassMap<Node> {
        public ParentMap() {
            Id(x => x.Id);
            Map(x => x.Name);
            References(x => x.Parent, "ParentId")
                .NotFound.Ignore();
        }
    }
```

3. Define your DbContext  
```c
    public class DbContext : NHibernateDbContext {
        private String _connectionString;

        public DbContext() {
            base.SetupConfiguration(Configure);
        }

        public DbContext(String connectionString) {
            _connectionString = connectionString;
            base.SetupConfiguration(Configure);
        }

        private NHibernate.Cfg.Configuration Configure() {
            var connectionString = _connectionString
                ?? ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            return Fluently.Configure()
                   .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DbContext>())
                .BuildConfiguration()
                /*.SetProperty(NHibernate.Cfg.Environment.ShowSql, Boolean.TrueString)*/;

        }
    }

```

4. Enjoy youself  

DbContext should be maintenance as singleton, dependency injection with autofac

```
    ContainerBuilder builder = ...
    builder.RegisterType<DbContext>().As<NHibernateDbContext>().SingleInstance();
```


4.1 Low-level api using NHibernate ISession
```c
        using (var context = new DbContext())
        using (var uow = new NHibernateUnitOfWork(context)) {           
            uow.Begin(); // using(uow.Begin()) is good choice
            var session = uow.OpenSession();
            var parent = new Node {
                Name = "Parent",
            };
            session.Save(parent);
            uow.Commit();

            var child = new Node {
                Name = "Child",
                Parent = parent,
            };
            session.Save(child);
            // Commit it, or lose your change
            uow.Commit(); 
            // Save you change using uow.Flush() without transaction
        }
4.2 Repository usage

```
    ...
    builder.RegisterGeneric(typeof(NHibernateRepository<>)).As(typeof(IRepository<>));
```



## Release log

[RELEASE_NOTES](RELEASE_NOTES.md)

