## Usage

### NHibernate

* 需要定义上下文, 从 NHibernateRepositoryContext 派生并提供 ISessionFactory 作为参数, 注意 NHibernate 的设计中 ISessionFactory 需要单例维护;
* 需要定义实体与 Mapping, 示例使用了 FluentNHibernate, 使用 xml 也无问题, 它们参与构建 ISessionFactory;

* [示例 PubsContext](src/PersistentDemo/MySql/PubsContext.cs)
* [示例 PubsEntites](src/PersistentDemo/MySql/PubsEntites.cs)
* [示例 PubsMappings](src/PersistentDemo/MySql/PubsMappings.cs)

ISession 结束时未提交变更默认被丢弃, 可以通过配置修改此行为

```xml
  <appSettings>
    <add key="NHibernate:alwaysCommit" value="true" />
	...
```

* alwaysCommit: false: 默认行为, 未提交变更将被丢弃;
* alwaysCommit: true : 如果当前 session 在事务管理下将提交变更; 如果当前不在事务的管理下则调用 ISession.Flush();


### Mongo

* 需要定义上下文, 从 MongoRepositoryContext 派生并提供连接字符串作为参数;
* 需要定义实体, 实体类与 documents 目前是简单映射, 待开发; 实体类需要从 IMongoId 或 IMongoId<T> 派生, 前者默认使用 ObjectId 作为主键类型;

* [示例 PubsContext](src/PersistentDemo/Mongo/PubsContext.cs);
* [示例 PubsEntities](src/PersistentDemo/Mongo/PubsEntities.cs);


## Release log

### v2.5.3

此版本起，nhibernate.session 结束时行未提交的变更默认将被丢弃，可以通过添加配置修改此行为

### v2.5

* 大幅简化了 NHibernate 相关实现, 移除了 IAggregate, 将 Id 的职责交还 NHibernate 的 Mapping 机制; 现在泛型 Repository 只在 Mongo 实现中保留;
* Retrive(Object[] keys) 与 Retrive<TMember>(String field, params TMember[] keys) 开始从具体实现引入抽象类 Repository<TEntry>;
  * NHibernate 以 ICriteria 特性实现;
  * Mongo 以携带 Id 的 IMongoId<TKey> 为基础实现;