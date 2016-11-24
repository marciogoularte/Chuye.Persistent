## 前言

- [jagregory/fluent-nhibernate](https://github.com/jagregory/fluent-nhibernate/wiki/Fluent-mapping)

Nhibernate 的Lazy load 对属性并不生效，这意味着以下 Poco 和 Mappinp 并不会延时加载
> One of the questions that is asked again and again in the NHibernate user mailing list is the question about whether NHibernate supports lazy-loading of properties. The answer is NO - at least for the time being.

* References
* HasMany
* HasOne
    * HasOne().Fetch.Select()
* CompositeId
* SubClass
* References, ManyToOne().LazyLoad()
* HasOne, .LazyLoad(Laziness.NoProxy)
    * Constrained()
    * PropertyRef()
* Summary


## References

### POCO 与 Mapping:

```c#
    class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual Drawer Drawer { get; set; }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            References(x => x.Desktop, "DesktopId")
                .NotFound.Ignore();
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            References(x => x.Drawer, "DrawerId")
                .Cascade.All()
                .NotFound.Ignore();
        }
    }
```

生成 schema 与存储数据并进行查询，使用 * 替换 NHibernate 生成的字段列表以方便阅读，附件见 [attach](http://note.youdao.com/noteshare?id=c5aee51cf7797964b9f1aed6272db06a)

```
    var context = new DbContext();
    /*
    drop table if exists `Desktop`

    drop table if exists `Drawer`

    create table `Desktop` (
        Id INTEGER not null,
       Title VARCHAR(255),
       DrawerId INTEGER,
       primary key (Id)
    )

    create table `Drawer` (
        Id INTEGER not null,
       Name VARCHAR(255),
       DesktopId INTEGER,
       primary key (Id)
    )    
    */
    
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var drawer = new Drawer {
            Id = 2,
            Name = "name"
        };
        var desktop = new Desktop() {
            Id = 3,
            Title = "title",
            Drawer = drawer,
        };
        drawer.Desktop = desktop;
        session.Save(desktop);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Desktop>(3)");
        session.Save(desktop);
    }
    
/*
NHibernate: SELECT drawer_.Id, drawer_.Name as Name1_, drawer_.DesktopId as DesktopId1_ FROM `Drawer` drawer_ WHERE drawer_.Id=?p0;?p0 = 2 [Type: Int32 (0)]

ISession.Save<Desktop>(3)
NHibernate: INSERT INTO `Drawer` (Name, DesktopId, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'name' [Type: String (4)], ?p1 = NULL [Type: Int32 (0)], ?p2 = 2 [Type: Int32 (0)]
NHibernate: INSERT INTO `Desktop` (Title, DrawerId, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'title' [Type: String (5)], ?p1 = 2 [Type: Int32 (0)], ?p2 = 3 [Type: Int32 (0)]
NHibernate: UPDATE `Drawer` SET Name = ?p0, DesktopId = ?p1 WHERE Id = ?p2;?p0 = 'name' [Type: String (4)], ?p1 = 3 [Type: Int32 (0)], ?p2 = 2 [Type: Int32 (0)]   
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_, desktop0_.DrawerId as DrawerId0_0_ FROM `Desktop` desktop0_ WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ FROM `Drawer` drawer0_ WHERE drawer0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
*/

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Drawer>(2);
    }
/*
ISession.Get<Drawer>(2)
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ FROM `Drawer` drawer0_ WHERE drawer0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_, desktop0_.DrawerId as DrawerId0_0_ FROM `Desktop` desktop0_ WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    
```

    
### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Drawer| Id | int(11) | NO | PRI
- | Name | varchar(255) | YES |	-
- | DesktopId | int(11) | YES |	-
Desktop | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | -
- | DrawerId | int(11) | YES | - 


### 查询步骤：

* 获取 Desktop(id:3) 包含2步
    * from Desktop, id=3
    * from Drawer, Id=2
* 获取 Drawer(id:2) 包含2步
    * from Drawer, Id=2
    * from Desktop, id=3
    

以上的场景有两个问题：

1. 无论是查询 Desktop(id=3)，还是查询 Drawer(id:2) 的 ，关联属性都被执行了查询；
2. 实际上一个 Desktop 可能有多个（HasMany） Drawer，但 Drawer 只能有一个（HasOne） Desktop

----


## HasMany

### POCO 与 Mapping:

```
    class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual IList<Drawer> Drawers { get; set; }

        public Desktop() {
            Drawers = new List<Drawer>();
        }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            References(x => x.Desktop, "DesktopId")
                .NotFound.Ignore();
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            HasMany(x => x.Drawers).KeyColumn("DesktopId")
                .Cascade.All();
        }
    }
```

将模型更贴合实例：Desktop 拥有多个 Drawer，Drawer 拥有 Desktop 的引用

```
    var context = new DbContext();
    
    /*
    drop table if exists `Desktop`

    drop table if exists `Drawer`

    create table `Desktop` (
        Id INTEGER not null,
       Title VARCHAR(255),
       primary key (Id)
    )

    create table `Drawer` (
        Id INTEGER not null,
       Name VARCHAR(255),
       DesktopId INTEGER,
       primary key (Id)
    )

    alter table `Drawer`
        add index (DesktopId),
        add constraint FKE918AB7AD1C83E28
        foreign key (DesktopId)
        references `Desktop` (Id)    
    */
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var drawer = new Drawer {
            Id = 2,
            Name = "name"
        };
        var desktop = new Desktop() {
            Id = 3,
            Title = "title",
        };
        drawer.Desktop = desktop;
        desktop.Drawers.Add(drawer);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Desktop>(3)");
        session.Save(desktop);
    }
/*
ISession.Save<Desktop>(3)
NHibernate: SELECT drawer_.Id, drawer_.Name as Name1_, drawer_.DesktopId as DesktopId1_ FROM `Drawer` drawer_ WHERE drawer_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
NHibernate: INSERT INTO `Desktop` (Title, Id) VALUES (?p0, ?p1);?p0 = 'title' [Type: String (5)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Drawer` (Name, DesktopId, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'name' [Type: String (4)], ?p1 = 3 [Type: Int32 (0)], ?p2 = 2 [Type: Int32 (0)]
NHibernate: UPDATE `Drawer` SET DesktopId = ?p0 WHERE Id = ?p1;?p0 = 3 [Type: Int32 (0)], ?p1 = 2 [Type: Int32 (0)]
*/

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_ FROM `Desktop` desktop0_ WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Drawer>(2);
    }
/*
ISession.Get<Drawer>(2)
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ FROM `Drawer` drawer0_ WHERE drawer0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_ FROM `Desktop` desktop0_ WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Drawer | Id | int(11) | NO | PRI
- | Name | varchar(255) | YES | 
- | DesktopId | int(11) | YES | MUL
Desktop | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | 



### 查询步骤：

* 获取 Desktop(id:3) 包含了1步，IList<Drawer> 被按预期 Lazy Load 了
    * from Desktop, id=3
* 获取 Drawer(id:2) 包含了2步
    * from Drawer, id=2
    * from Desktop id=3

考虑到 DDD 中==聚合根==概念，常常不为 Drawer 提供单独的仓储接口，故 Drawer 的获取包含了2步并不是很大的问题。

----

## HasOne 

> To specify the foreign key, you can use the PropertyRef() method. Additionally, if you are using References() to specify the bi-directional relationship on the other end of the relationship, be sure to use the .Unique() mapping modifier to effectively specify that this is a one-to-one relationship.

修改 Drawer 为 HasOne(Desktop)，重试请求，注意 schema 产生了 ```UNIQUE```，查询语句出现了 ```LEFT JOIN```

### POCO 与 Mapping:

```
    class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual Drawer Drawer { get; set; }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            References(x => x.Desktop, "DesktopId")
                .Unique()
                .NotFound.Ignore();
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            HasOne(x => x.Drawer).PropertyRef(x => x.Desktop)
                //.Fetch.Join()
                .Cascade.All();
        }
    }
```

```
    var context = new DbContext();
    /*
    drop table if exists `Desktop`

    drop table if exists `Drawer`

    create table `Desktop` (
        Id INTEGER not null,
       Title VARCHAR(255),
       primary key (Id)
    )

    create table `Drawer` (
        Id INTEGER not null,
       Name VARCHAR(255),
       DesktopId INTEGER unique,
       primary key (Id)
    )
    */
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var drawer = new Drawer {
            Id = 2,
            Name = "name"
        };
        var desktop = new Desktop() {
            Id = 3,
            Title = "title",
        };
        drawer.Desktop = desktop;
        desktop.Drawer = drawer;
        session.Save(desktop);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Desktop>(3)");
        session.Save(desktop);
    }
/*
NHibernate: SELECT drawer_.Id, drawer_.Name as Name1_, drawer_.DesktopId as DesktopId1_ FROM `Drawer` drawer_ WHERE drawer_.Id=?p0;?p0 = 2 [Type: Int32 (0)]

ISession.Save<Desktop>(3)
NHibernate: INSERT INTO `Desktop` (Title, Id) VALUES (?p0, ?p1);?p0 = 'title' [Type: String (5)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Drawer` (Name, DesktopId, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'name' [Type: String (4)], ?p1 = 3 [Type: Int32 (0)], ?p2 = 2 [Type: Int32 (0)]
*/

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_1_, desktop0_.Title as Title0_1_, drawer1_.Id as Id1_0_, drawer1_.Name as Name1_0_, drawer1_.DesktopId as DesktopId1_0_ FROM `Desktop` desktop0_ left outer join `Drawer` drawer1_ on desktop0_.Id=drawer1_.DesktopId WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Drawer>(2);
    }
    
/*
ISession.Get<Drawer>(2)
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ FROM `Drawer` drawer0_ WHERE drawer0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
NHibernate: SELECT desktop0_.Id as Id0_1_, desktop0_.Title as Title0_1_, drawer1_.Id as Id1_0_, drawer1_.Name as Name1_0_, drawer1_.DesktopId as DesktopId1_0_ FROM `Desktop` desktop0_ left outer join `Drawer` drawer1_ on desktop0_.Id=drawer1_.DesktopId WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ FROM `Drawer` drawer0_ WHERE drawer0_.DesktopId=?p0;?p0 = 3 [Type: Int32 (0)]
*/    
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Drawer | Id | int(11) | NO | PRI
- | Name | varchar(255) | YES | 
- | DesktopId | int(11) | YES | UNI
Desktop | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | 

### 查询步骤：

* 获取 Desktop(id:3) 包含1步
    * from Desktop left join Drawer, Desktop.Id=3
* 获取 Drawer(id:2) 包含3步
    * from Drawer, id=2
    * from Desktop left join Drawer, Desktop.Id=3
    * from Drawer DesktopId=3

### HasOne().Fetch.Select()

可以通过添加 ```OneToOnePart<TOther>.Fetch.Select()``` 可以改变查询方式从 LEFT JOIN 到 id 查询

```
    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            HasOne(x => x.Drawer).PropertyRef(x => x.Desktop)
                .Fetch.Select()
                .Cascade.All();
        }
    }      
```

```
    var context = new DbContext();
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        ...
    
    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_ 
FROM `Desktop` desktop0_ 
WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ 
FROM `Drawer` drawer0_ 
WHERE drawer0_.DesktopId=?p0;?p0 = 3 [Type: Int32 (0)]
*/

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Drawer>(2);
    }
/*  
ISession.Get<Drawer>(2)
NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ 
FROM `Drawer` drawer0_ 
WHERE drawer0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]

NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_ 
FROM `Desktop` desktop0_ 
WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

NHibernate: SELECT drawer0_.Id as Id1_0_, drawer0_.Name as Name1_0_, drawer0_.DesktopId as DesktopId1_0_ 
FROM `Drawer` drawer0_ 
WHERE drawer0_.DesktopId=?p0;?p0 = 3 [Type: Int32 (0)]
 */
```

### 查询步骤：

* 获取 Desktop(id:3) 包含2步
    * from Desktop, Id=3
    * from Drawer, DesktopId=3
* 获取 Drawer(id:2) 包含3步
    * from Drawer, id=2
    * from Desktop, Id=3
    * from Drawer， DesktopId=3

可以观察到，==HasOne 映射修改了 Drawer 的查询方式，并将该方式运用到 Desktop.Drawer 属性产生的查询中==：默认的 .Fetch.Join() 使用了 LEFT JOIN， .Fetch.Select() 则是使用额外的查询；

----

## CompositeId 

拥有相同主键的实体可以使用 CompositeId 进行 mapping，例如 User-Id 拥有的 Account-Id 相同的情况

### POCO 与 Mapping:

```
    class Drawer {
        public virtual Desktop Desktop { get; set; }
        public virtual String Name { get; set; }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var child = obj as Drawer;

            if (child != null && child.Desktop != null) {
                return child.Desktop.Id == Desktop.Id;
            }

            return false;
        }

        public override int GetHashCode() {
            return Desktop.Id;
        }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            CompositeId().KeyReference(x => x.Desktop, "Id");
            Map(x => x.Name);
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
        }
    }
```

Poco 依赖 Equals, GetHashCode 的重写，见 [Equals and Hashcode](http://www.laliluna.de/jpa-hibernate-guide/ch06s06.html)

> A composite id must implement both methods or the id will not work properly. Hibernate must be able to compare ids. A composite id will most likely use all its id fields in the equals and hashCode methods.

```
    var context = new DbContext();
/*
    drop table if exists `Desktop`

    drop table if exists `Drawer`

    create table `Desktop` (
        Id INTEGER not null,
       Title VARCHAR(255),
       primary key (Id)
    )

    create table `Drawer` (
        Id INTEGER not null,
       Name VARCHAR(255),
       primary key (Id)
    )

    alter table `Drawer`
        add index (Id),
        add constraint FKE918AB7A416C888F
        foreign key (Id)
        references `Desktop` (Id)
*/    
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = new Desktop() {
            Id = 3,
            Title = "title",
        };
        session.Save(desktop);
        var drawer = new Drawer {
            Desktop = desktop,
            Name = "name",
        };
        session.Save(drawer);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Desktop>(3)");        
    }
/*
ISession.Save<Desktop>(3)
NHibernate: INSERT INTO `Desktop` (Title, Id) VALUES (?p0, ?p1);?p0 = 'title' [Type: String (5)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Drawer` (Name, Id) VALUES (?p0, ?p1);?p0 = 'name' [Type: String (4)], ?p1 = 3 [Type: Int32 (0)]
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Title as Title0_0_ FROM `Desktop` desktop0_ WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        //var desktop = session.Get<Drawer>(3);
    }
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
 Desktop | Id | int(11) | NO | PRI
 - | Name | varchar(255) | YES | 
 Drawer | Id | int(11) | NO | PRI
 - | Title | varchar(255) | YES | 


该策略带来了 DB 层面的最小开销，但失去了通过 id 查询 Drawer 的能力，我们需要为 Drawer 添加额外的且不同的 Mapping 来完成相关需求，且 Equals(), GetHashCode() 的重写十分丑陋。


## SubClass

对于Id 共享的另一种方案是 Subclass mapping，比如产品有各种具体类别的子产品

```
    class Desktop  {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
    }

    class Drawer : Desktop {
        public virtual String Title { get; set; }
    }

    class DesktopMap : ClassMap <Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
        }
    }

    class DrawerMap : SubclassMap<Drawer> {
        public DrawerMap() {
            KeyColumn("Id");
            Map(x => x.Title);
        }
    }
```

```
    var context = new DbContext();
/*
    drop table if exists `Desktop`

    drop table if exists `Drawer`

    create table `Desktop` (
        Id INTEGER not null,
       Name VARCHAR(255),
       primary key (Id)
    )

    create table `Drawer` (
        Id INTEGER not null,
       Title VARCHAR(255),
       primary key (Id)
    )

    alter table `Drawer`
        add index (Id),
        add constraint FKE918AB7A416C888F
        foreign key (Id)
        references `Desktop` (Id)
*/    

    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var drawer = new Drawer {
            Id = 3,
            Name = "name",
            Title = "title",
        };
        session.Save(drawer);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Desktop>(3)");        
    }
/*
ISession.Save<Desktop>(3)
NHibernate: INSERT INTO `Desktop` (Name, Id) VALUES (?p0, ?p1);?p0 = 'name' [Type: String (4)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Drawer` (Title, Id) VALUES (?p0, ?p1);?p0 = 'title' [Type: String (5)], ?p1 = 3 [Type: Int32 (0)]
*/    

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Desktop>(3)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Desktop>(3);
    }
/*
ISession.Get<Desktop>(3)
NHibernate: SELECT desktop0_.Id as Id0_0_, desktop0_.Name as Name0_0_, desktop0_1_.Title as Title1_0_, 
    case 
    when desktop0_1_.Id is not null then 1 
    when desktop0_.Id is not null then 0 
    end as clazz_0_ 
FROM `Desktop` desktop0_ 
left outer join `Drawer` desktop0_1_ on desktop0_.Id=desktop0_1_.Id 
WHERE desktop0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/

    Console.WriteLine();
    Console.WriteLine("ISession.Get<Drawer>(2)");
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var desktop = session.Get<Drawer>(3);
    }
/*
ISession.Get<Drawer>(2)
NHibernate: SELECT drawer0_.Id as Id0_0_, drawer0_1_.Name as Name0_0_, drawer0_.Title as Title1_0_ 
FROM `Drawer` drawer0_ 
inner join `Desktop` drawer0_1_ on drawer0_.Id=drawer0_1_.Id 
WHERE drawer0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/    
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
 Desktop | Id | int(11) | NO | PRI
 - | Name | varchar(255) | YES | 
 Drawer | Id | int(11) | NO | PRI
 - | Title | varchar(255) | YES | 


### 查询步骤:

单操作子类，父类自动保存，id 沿用到子类，但查询出来了 ```case when``` 与 ```left join```, ```inner join```， 由于所有操作同时读写两张表，其开销需要评估

## References, ManyToOne().LazyLoad()

LazyLoad 默认对 HasMany 生效，但“属性的延迟加载”资料有限，以下是个人摸索的前提与结论

* 实例 Poco 需要使用 public 修饰
* ISession.Get<T>() 和 ISession.Load<T>() 都能工作
* 修改 References 即 ManyToOne().LazyLoad(Laziness.NoProxy)，并注意 LazyLoad 对 Not 不感冒
* ISession.Transaction.Commit 时，未延迟加载的对象会被加载出来，推测是避免已变更属性未被持久化所需的对比，导致关联数据被查询；该行为可以通过 Transaction.Roll() 或者 ISession.Clear() 修改；

one-to-one 的延迟加载问题见老赵的 [NHibernate中一对一关联的延迟加载](http://www.cnblogs.com/JeffreyZhao/archive/2009/08/17/lazy-load-of-one-to-one-association-in-nhibernate.html)

> Specify the lazy behaviour of this relationship. Cannot be used with the FluentNHibernate.Mapping.ManyToOnePart`1.Not modifier.

### POCO 与 Mapping:

```
    public class Cover {
        public virtual Int32 Id { get; set; }
        public virtual String Picture { get; set; }
    }

    public class Book {
        public virtual Int32 Id { get; set; }
        public virtual String Author { get; set; }
        public virtual String Title { get; set; }
        public virtual Cover Cover { get; set; }
    }

    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            References(x => x.Cover, "CoverId")
                .LazyLoad(Laziness.NoProxy)
                .Unique()
                .Cascade.All()
                .NotFound.Ignore();
        }
    }
```

```
    drop table if exists `Book`

    drop table if exists `Cover`

    create table `Book` (
        Id INTEGER not null,
       Title VARCHAR(255),
       Author VARCHAR(255),
       CoverId INTEGER unique,
       primary key (Id)
    )

    create table `Cover` (
        Id INTEGER not null,
       Picture VARCHAR(255),
       primary key (Id)
    )
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Book | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | 
- | Author | varchar(255) | YES | 
- | CoverId | int(11) | YES | UNI
Cover | Id | int(11) | NO | PRI
- | Picture | varchar(255) | YES | 

### 插入数据
```
    var context = new DbContext();
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var cover = new Cover {
            Id = 2,
            Picture = "picture",
        };
        var book = new Book() {
            Id = 3,
            Author = "jusfrw",
            Title = "Title",
            Cover = cover,
        };
        session.Save(book);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Book>(3)");
    }
/*
NHibernate: SELECT cover_.Id, cover_.Picture as Picture1_ FROM `Cover` cover_ WHERE cover_.Id=?p0;?p0 = 2 [Type: Int32 (0)]

ISession.Save<Book>(3)
NHibernate: INSERT INTO `Cover` (Picture, Id) VALUES (?p0, ?p1);?p0 = 'picture' [Type: String (7)], ?p1 = 2 [Type: Int32 (0)]
NHibernate: INSERT INTO `Book` (Title, Author, CoverId, Id) VALUES (?p0, ?p1, ?p2, ?p3);?p0 = 'Title' [Type: String (5)], ?p1 = 'jusfrw' [Type: String (6)], ?p2 = 2 [Type: Int32 (0)], ?p3 = 3 [Type: Int32 (0)]

*/
```

由访问 Book.Cover.Id 后打印的SQL 可知，延迟加载生效了

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);

        Console.WriteLine("Book.Cover.Id: {0}", book.Cover.Id);
        Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
    }
/*
ISession.Get<Book>(3) with trans commit
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_, book0_.CoverId as CoverId0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Id: 3
Book.Title: Title
NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ FROM `Cover` cover0_ WHERE cover0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
Book.Cover.Id: 2
Book.Cover.Picture: picture
*/
```

在事务下测试延迟加载，可见 Transaction.Commit() 调用时，被迟延的实体引起了额外的查询语句

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        Console.WriteLine("eof");
        //Console.WriteLine("Book.Cover.Id: {0}", book.Cover.Id);
        //Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
    }
    
/*
ISession.Get<Book>(3) with trans commit
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_, book0_.CoverId as CoverId0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Id: 3
Book.Title: Title
eof
NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ FROM `Cover` cover0_ WHERE cover0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
*/    
```

在事务下测试延迟加载，未访问被延迟属性，在 Transaction.Commit() 前调用 ISession.Clear()

```
    using (var uow = new NHibernateUnitOfWork(context)) {
        uow.Begin();
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans");
    
        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        session.Clear();
        Console.WriteLine("ISession.Clear()");
        //Console.WriteLine("Book.Cover.Id: {0}", book.Picture.Id);
        //Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
    }

/*
ISession.Get<Book>(3) with trans
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_, book0_.CoverId as CoverId0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Id: 3
Book.Title: Title
ISession.Clear()
*/
```

非事务下未查询被延迟加载的对象

```
    using (var uow = new NHibernateUnitOfWork(context))
    /*using (uow.Begin())*/ {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) without trans");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        Console.WriteLine("eof");
    }
            
/*            
ISession.Get<Book>(3) without trans
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_, book0_.CoverId as CoverId0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
book.Id 3
book.Title: Title
eof
*/
```

---- 

## HasOne, .LazyLoad(Laziness.NoProxy), Constrained() or PropertyRef()

HasOne() 同样能使用延迟加载，但需要配合 Constrained() 完成 mapping

### POCO 与 Mapping

* 延迟加载要求实体是 public 修饰的
* Constrained() 使得子级沿用父级主键

```
    public class Cover {
        public virtual Int32 Id { get; set; }
        public virtual String Picture { get; set; }
    }

    public class Book {
        public virtual Int32 Id { get; set; }
        public virtual String Author { get; set; }
        public virtual String Title { get; set; }
        public virtual Cover Cover { get; set; }
    }

    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            HasOne(x => x.Cover)
                .LazyLoad(Laziness.NoProxy)
                .Constrained()
                .Cascade.All();
        }
    }
/*
    drop table if exists `Book`

    drop table if exists `Cover`

    create table `Book` (
        Id INTEGER not null,
       Title VARCHAR(255),
       Author VARCHAR(255),
       primary key (Id)
    )

    create table `Cover` (
        Id INTEGER not null,
       Picture VARCHAR(255),
       primary key (Id)
    )

    alter table `Book`
        add index (Id),
        add constraint FKBB0005CDB86BBB7E
        foreign key (Id)
        references `Cover` (Id)
*/    
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Book | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | 
- | Author | varchar(255) | YES | 
Cover | Id | int(11) | NO | PRI
- | Picture | varchar(255) | YES | 


持久化数据
```
    var context = new DbContext();
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var cover = new Cover {
            Id = 3,
            Picture = "picture",
        };
        var book = new Book() {
            Id = 3,
            Author = "jusfrw",
            Title = "Title",
            Cover = cover,
        };
        session.Save(book);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Book>(3)");
    }
/*
NHibernate: SELECT cover_.Id, cover_.Picture as Picture1_ FROM `Cover` cover_ WHERE cover_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

ISession.Save<Book>(3)
NHibernate: INSERT INTO `Cover` (Picture, Id) VALUES (?p0, ?p1);?p0 = 'picture' [Type: String (7)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Book` (Title, Author, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'Title' [Type: String (5)], ?p1 = 'jusfrw' [Type: String (6)], ?p2 = 3 [Type: Int32 (0)]
*/           
```

事务下的延迟加载

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);

        Console.WriteLine("Book.Cover.Id: {0}", book.Cover.Id);
        Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
    }
/*            
ISession.Get<Book>(3) with trans commit
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_ 
FROM `Book` book0_ 
WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

Book.Id: 3
Book.Title: Title

NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ FROM `Cover` cover0_ 
WHERE cover0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Cover.Id: 3
Book.Cover.Picture: picture
*/
```

事务管理下延迟加载的实体，即使没有被通过属性访问，仍然被查询出来

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit");
    
        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        Console.WriteLine("eof");
        //Console.WriteLine("book.Cover.Id: {0}", book.Cover.Id);
        //Console.WriteLine("book.Cover.Picture: {0}", book.Cover.Picture);
    }
/*
ISession.Get<Book>(3) with trans commit
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_ 
FROM `Book` book0_ 
WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

Book.Id: 3
Book.Title: Title
eof

NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ 
FROM `Cover` cover0_ 
WHERE cover0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/
```

ISession.Clear 的使用阻止后续被延迟对象的访问

```
    using (var uow = new NHibernateUnitOfWork(context)) {
        uow.Begin();
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit, but ISession.Clear()");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        session.Clear();
        Console.WriteLine("ISession.Clear()");
        //Console.WriteLine("book.Cover.Id: {0}", book.Picture.Id);
        //Console.WriteLine("book.Cover.Picture: {0}", book.Cover.Picture);
    }
/*
ISession.Get<Book>(3) with trans commit, but ISession.Clear()
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_ 
FROM `Book` book0_ 
WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

Book.Id: 3
Book.Title: Title
ISession.Clear()
*/            
```

某些已经存在的表中，子级不沿用父级主键，此时需要修改 Constrained() 为 PropertyRef()

### POCO 与 Mapping

```
    public class Cover {
        public virtual Int32 Id { get; set; }
        public virtual String Picture { get; set; }
        public virtual Book Book { get; set; }
    }

    public class Book {
        public virtual Int32 Id { get; set; }
        public virtual String Author { get; set; }
        public virtual String Title { get; set; }
        public virtual Cover Cover { get; set; }
    }

    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
            References(x => x.Book, "BookId");
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            HasOne(x => x.Cover)
                .LazyLoad(Laziness.NoProxy)
                //.Constrained()
                .PropertyRef(x => x.Book)
                .Cascade.All();
        }
    }

/*
    drop table if exists `Book`

    drop table if exists `Cover`

    create table `Book` (
        Id INTEGER not null,
       Title VARCHAR(255),
       Author VARCHAR(255),
       primary key (Id)
    )

    create table `Cover` (
        Id INTEGER not null,
       Picture VARCHAR(255),
       BookId INTEGER,
       primary key (Id)
    )

    alter table `Cover`
        add index (BookId),
        add constraint FK4A41A89D10FCD5DA
        foreign key (BookId)
        references `Book` (Id)
*/    
```

### 生成的 schema:

Table| Field | Type | Null | Key
---|---|---|---|---
Book | Id | int(11) | NO | PRI
- | Title | varchar(255) | YES | 
- | Author | varchar(255) | YES | 
Cover | Id | int(11) | NO | PRI
- | Picture | varchar(255) | YES | 
- | BookId | int(11) | YES | MUL


持久化

```
    var context = new DbContext();
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var cover = new Cover {
            Id = 2,
            Picture = "picture",
        };
        var book = new Book() {
            Id = 3,
            Author = "jusfrw",
            Title = "Title",
            Cover = cover,
        };
        cover.Book = book;
        session.Save(book);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Book>(3)");
    }
/*
NHibernate: SELECT cover_.Id, cover_.Picture as Picture1_, cover_.BookId as BookId1_ FROM `Cover` cover_ WHERE cover_.Id=?p0;?p0 = 2 [Type: Int32 (0)]

ISession.Save<Book>(3)
NHibernate: INSERT INTO `Book` (Title, Author, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'Title' [Type: String (5)], ?p1 = 'jusfrw' [Type: String (6)], ?p2 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Cover` (Picture, BookId, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'picture' [Type: String (7)], ?p1 = 3 [Type: Int32 (0)], ?p2 = 2 [Type: Int32 (0)]
*/
```

注意，获取父级使用了 ```left join```

```
/*
    using (var uow = new NHibernateUnitOfWork(context))
    /*using (uow.Begin())*/ {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) without trans");
    
        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
    
        Console.WriteLine("Book.Cover.Id: {0}", book.Cover.Id);
        Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
            }
            
ISession.Get<Book>(3) without trans
NHibernate: SELECT book0_.Id as Id0_1_, book0_.Title as Title0_1_, book0_.Author as Author0_1_, cover1_.Id as Id1_0_, cover1_.Picture as Picture1_0_, cover1_.BookId as BookId1_0_ 
FROM `Book` book0_ 
left outer join `Cover` cover1_ on book0_.Id=cover1_.BookId 
WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

Book.Id: 3
Book.Title: Title
Book.Cover.Id: 2
Book.Cover.Picture: picture
*/
```

获取子级使用了延时加载

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Cover>(3) with trans commit");
    
        var session = uow.OpenSession();
        var cover = session.Get<Cover>(2);
        Console.WriteLine("Cover.Id: {0}", cover.Id);
        Console.WriteLine("Cover.Picture: {0}", cover.Picture);
        
        Console.WriteLine("Cover.Book.Id: {0}", cover.Book.Id);
        Console.WriteLine("Cover.Book.Picture: {0}", cover.Book.Author);
    }
/*
ISession.Get<Cover>(3) with trans commit
NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_, cover0_.BookId as BookId1_0_ 
FROM `Cover` cover0_ WHERE cover0_.Id=?p0;?p0 = 2 [Type: Int32 (0)]
Cover.Id: 2
Cover.Picture: picture

NHibernate: SELECT book0_.Id as Id0_1_, book0_.Title as Title0_1_, book0_.Author as Author0_1_, cover1_.Id as Id1_0_, cover1_.Picture as Picture1_0_, cover1_.BookId as BookId1_0_ 
FROM `Book` book0_ 
left outer join `Cover` cover1_ on book0_.Id=cover1_.BookId 
WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_, cover0_.BookId as BookId1_0_ 
FROM `Cover` cover0_ 
WHERE cover0_.BookId=?p0;?p0 = 3 [Type: Int32 (0)]

Cover.Book.Id: 3
Cover.Book.Picture: jusfrw
*/
```

事实上子级对父级的引用可能只保存值字段

### POCO 与 Mapping

此时子级失去了对父级的访问能力, schema 没有变动，对父级的访问仍然会出现 ```left join```

```
    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
            Map(x => x.BookId);
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            HasOne(x => x.Cover)
                .LazyLoad(Laziness.NoProxy)
                //.Constrained()
                .PropertyRef(x => x.BookId)
                .Cascade.All();
        }
    }
```


回头看 Constrained()，与 PropertyRef() 的区别在子级未引用父级，事实上如果双向使用 HasOne().Constrained()，会导致无法插入数据。

### POCO 与 Mapping
```
    public class Cover {
        public virtual Int32 Id { get; set; }
        public virtual String Picture { get; set; }
        public virtual Book Book { get; set; }
    }

    public class Book {
        public virtual Int32 Id { get; set; }
        public virtual String Author { get; set; }
        public virtual String Title { get; set; }
        public virtual Cover Cover { get; set; }
    }

    class CoverMap : ClassMap<Cover> {
        public CoverMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Picture);
            HasOne(x => x.Book)
                .LazyLoad(Laziness.NoProxy)
                .Constrained()
                .Cascade.All();
        }
    }

    class BookMap : ClassMap<Book> {
        public BookMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            Map(x => x.Author);
            HasOne(x => x.Cover)
                .LazyLoad(Laziness.NoProxy)
                .Constrained()
                .Cascade.All();
        }
    }

/*
    drop table if exists `Book`

    drop table if exists `Cover`

    create table `Book` (
        Id INTEGER not null,
       Title VARCHAR(255),
       Author VARCHAR(255),
       primary key (Id)
    )

    create table `Cover` (
        Id INTEGER not null,
       Picture VARCHAR(255),
       primary key (Id)
    )

    alter table `Book`
        add index (Id),
        add constraint FKBB0005CDB86BBB7E
        foreign key (Id)
        references `Cover` (Id)

    alter table `Cover`
        add index (Id),
        add constraint FK4A41A89DAEE8DC07
        foreign key (Id)
        references `Book` (Id)
*/
```

父级主键与子级主键互为外键时，无论先插入哪方数据，必然导致约束不能满足
```
    var context = new DbContext();
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        var session = uow.OpenSession();
        var cover = new Cover {
            Id = 3,
            Picture = "picture",
        };
        var book = new Book() {
            Id = 3,
            Author = "jusfrw",
            Title = "Title",
            Cover = cover,
        };
        cover.Book = book;
        session.Save(book);

        Console.WriteLine();
        Console.WriteLine("ISession.Save<Book>(3)");
    }
NHibernate: SELECT cover_.Id, cover_.Picture as Picture1_ FROM `Cover` cover_ WHERE cover_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

ISession.Save<Book>(3)
NHibernate: INSERT INTO `Cover` (Picture, Id) VALUES (?p0, ?p1);?p0 = 'picture' [Type: String (7)], ?p1 = 3 [Type: Int32 (0)]

Unhandled Exception: NHibernate.Exceptions.GenericADOException: could not insert: [PersistentDemo.Models.Cover#3][SQL: INSERT INTO `Cover` (Picture, Id) VALUES (?, ?)] ---> MySql.Data.MySqlClient.MySqlException: Cannot add or update a child row: a foreign key constraint fails (`test`.`cover`, CONSTRAINT `FK4A41A89DAEE8DC07` FOREIGN KEY (`Id`) REFERENCES `book` (`Id`))
```

解决思路是放弃数据库的事实约束

### POCO 与 Mapping

```
    drop table if exists `Book`

    drop table if exists `Cover`

    create table `Book` (
        Id INTEGER not null,
       Title VARCHAR(255),
       Author VARCHAR(255),
       primary key (Id)
    )

    create table `Cover` (
        Id INTEGER not null,
       Picture VARCHAR(255),
       primary key (Id)
    )
    
NHibernate: SELECT cover_.Id, cover_.Picture as Picture1_ FROM `Cover` cover_ WHERE cover_.Id=?p0;?p0 = 3 [Type: Int32 (0)]

ISession.Save<Book>(3)
NHibernate: INSERT INTO `Cover` (Picture, Id) VALUES (?p0, ?p1);?p0 = 'picture' [Type: String (7)], ?p1 = 3 [Type: Int32 (0)]
NHibernate: INSERT INTO `Book` (Title, Author, Id) VALUES (?p0, ?p1, ?p2);?p0 = 'Title' [Type: String (5)], ?p1 = 'jusfrw' [Type: String (6)], ?p2 = 3 [Type: Int32 (0)]
```
后续的延迟加载通过了测试

```
    using (var uow = new NHibernateUnitOfWork(context))
    using (uow.Begin()) {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) with trans commit");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);
        Console.WriteLine("eof");
        //Console.WriteLine("book.Cover.Id: {0}", book.Cover.Id);
        //Console.WriteLine("book.Cover.Picture: {0}", book.Cover.Picture);
    }
    
/*
ISession.Get<Book>(3) with trans commit
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Id: 3
Book.Title: Title
eof
NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ FROM `Cover` cover0_ WHERE cover0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
*/

    using (var uow = new NHibernateUnitOfWork(context))
    /*using (uow.Begin())*/ {
        Console.WriteLine();
        Console.WriteLine("ISession.Get<Book>(3) without trans");

        var session = uow.OpenSession();
        var book = session.Get<Book>(3);
        Console.WriteLine("Book.Id: {0}", book.Id);
        Console.WriteLine("Book.Title: {0}", book.Title);

        Console.WriteLine("Book.Cover.Id: {0}", book.Cover.Id);
        Console.WriteLine("Book.Cover.Picture: {0}", book.Cover.Picture);
    }
/*    
ISession.Get<Book>(3) without trans
NHibernate: SELECT book0_.Id as Id0_0_, book0_.Title as Title0_0_, book0_.Author as Author0_0_ FROM `Book` book0_ WHERE book0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Id: 3
Book.Title: Title
NHibernate: SELECT cover0_.Id as Id1_0_, cover0_.Picture as Picture1_0_ FROM `Cover` cover0_ WHERE cover0_.Id=?p0;?p0 = 3 [Type: Int32 (0)]
Book.Cover.Id: 3
Book.Cover.Picture: picture
*/
```

并没有产生 left join 一类查询

## Summary
* HasMany 默认延迟加载
* References/HasOne 可以通过 .LazyLoad(Laziness.NoProxy) 实现延迟加载
* References/HasOne 延迟加载需要处理事务控制下 ISession释放前会加载已经被延迟访问的对象的问题
* HasOne 比较特别
    * 单向的 HasOne().Constrained() 即子级不引用父级是很好的处理方案
    * 双向的 HasOne().PropertyRef() + References() 方案下，父级查询出现 left join 子级；
    * 放弃数据库的事实约束，采用逻辑上的 HasOne().Constrained() + HasOne().Constrained()，能够避免上述问题

业务中如果出现 HasOne 场景，比如每条订单有一条支付记录、每本书有一个封面，那么共用主键配合 HasOne().Constrained() 能够带来逻辑简化和性能优化；

子级对父级的引用只保留父级主键的场景仍然是有意义的，比如每个订单只有一条物流记录，而物流记录是继承表关系，那么可以在订单记录中存储根级物流记录id，根级物流记录记录中存储物流类型，实现物流记录的分类别获取；

在有历史遗留问题的系统里，比如已经有一套订单在运作，新业务中新订单记录与新支付记录使用 HasOne 共用Id的情况下，很容易出现新旧支付记录主键冲突的总理，需要在新业务上线前 ```SET SET AUTO_INCREMENT```留下旧数据迁移空间；
