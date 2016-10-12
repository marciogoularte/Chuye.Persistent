## Usage
### Chuye.Persistent.PetaPoco

1. Add dependency using nuget  

```bash
    install-package Chuye.Persistent.PetaPoco
```

2. Add poco，several attributes provided for table, primaryKey customization  

```c
    [PetaPoco.PrimaryKey("Id", AutoIncrement = true)]
    public class Person {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual DateTime Birth { get; set; }
        public virtual String Address { get; set; }
        public virtual Int32 Job_id { get; set; }
    }
```

3. PetaPocoUnitOfWork associated with PetaPoco.Database instance implemented IUnitOfWork interface, should maintained as build-in life cycleined inline or PerHttpRequest with dependency injection tool. PetaPocoDbContext is a optional lightweight object holding the connection information. All operations should be organised around the PetaPoco.Databas instance, which can be obtained from the PetaPocoDbContext.Database, is a extended implements, intercepting exception for transaction management.

```c
    //work with PetaPoco.Database
    var uow = new PetaPocoUnitOfWork("connectionStringName");
    var list = uow.Database.Fetch<Person>("select * from person limit 10");

    //transaction behaviour with using, commit if no error, or roll back
    using (uow.Begin()) {
        uow.Database.Execute("update person set job_id = job_id + 1");
        uow.Database.Execute("update person set name = 'too longggggggggggggg'"); //Data too long for column 'Name' at row 1
    }    

    //paging sample
    Object lastId = 0;
    //sql must use inequality-equation and limit
    var sql = "select * from person where id > @0 order by id asc limit 100"; 
    var page = uow.Database.DeferPage<Person>(sql, x => x.Max(z => z.Id));
    while (page.Next(ref lastId)) {
        //do stuff with page.Items
    }
```

> Diagnostic message

```c
    Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
```

### Chuye.Persistent.NHibernate

1. Add dependency using nuget  

```bash
    install-package FluentNHibernate
    install-package Chuye.Persistent.NHibernate
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

4. DbContext should be maintenance as singleton, dependency injection with autofac, Enjoy youself  

```c
    ContainerBuilder builder = ...
    builder.RegisterType<DbContext>().As<NHibernateDbContext>().SingleInstance();
```


> Low-level api using NHibernate ISession

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
```

> Optional repository usage, sample of Autofac

```
    builder.RegisterGeneric(typeof(NHibernateRepository<>)).As(typeof(IRepository<>));
```

## Release log

[RELEASE_NOTES](/blob/master/RELEASE_NOTES.md)

