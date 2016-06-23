## Usage

### NHibernate

* 需要定义上下文, 从 NHibernateRepositoryContext 派生并提供 ISessionFactory;
* 需要定义实体与 Mapping, 使用 xml 或者 FluentNHibernate 用于构建 ISessionFactory;

> 注意 NHibernate 的设计中 ISessionFactory 需要单例维护

#### 示例

* [PubsContext](src/PersistentDemo/MySql/PubsContext.cs)
* [PubsEntites](src/PersistentDemo/MySql/PubsEntites.cs)
* [PubsMappings](src/PersistentDemo/MySql/PubsMappings.cs)

ISession.Dispose() 时未提交变依赖 NHibernate 行为策略, 可以通过配置修改此行为;

```xml
  <appSettings>
    <add key="NHibernate:alwaysCommit" value="true" />
	...
```

* alwaysCommit: false 或者缺失, 当前 ISession 在事务管理下则回滚事务, 否则遵从 NHibernate 策略;
* alwaysCommit: true  当前 ISession 在事务管理下则提交事务, 更否则调用 ISession.Flush();


### Mongo

* 需要定义上下文, 从 MongoRepositoryContext 派生并提供 connectionString;
* 需要定义实体, 从 IMongoId 或 IMongoId<T> 定义, 前者默认使用 ObjectId 作为主键类型;
* 实体类与 documents 目前是简单映射, 待计划;

#### 示例

* [PubsContext](src/PersistentDemo/Mongo/PubsContext.cs)
* [PubsEntities](src/PersistentDemo/Mongo/PubsEntities.cs)


## Release log

### v2.5

* Nhibernate.Session 结束时未提交变更依赖 NHibernate 行为策略，可以通过添加配置修改此行为;
* 大幅简化了 NHibernate 相关实现, 移除了 IAggregate, 将 Id 的职责交还 NHibernate 的 Mapping 机制; 现在泛型 Repository 只在 Mongo 实现中保留;
* Retrive(Object[] keys) 与 Retrive<TMember>(String field, params TMember[] keys) 开始从具体实现引入抽象类 Repository<TEntry>;
  * NHibernate 以 ICriteria 特性实现;
  * Mongo 以携带 Id 的 IMongoId<TKey> 为基础实现;