## Release log

### v2.5.2

此版本起，nhibernate.session 结束时行未提交的变更默认将被丢弃，可以通过可选的添加配置文件修改此行为

```xml
  <configSections>
    <section name="nhibernateBehaviour" type="Chuye.Persistent.NHibernate.NhibernateBehaviourConfigurationSection, Chuye.Persistent.NHibernate" />
  </configSections>
  <nhibernateBehaviour alwaysCommit="false" />
```

* alwaysCommit="false": 默认行为, 所有未提交变更将被丢弃;
* alwaysCommit="true":  如果当前 session 在事务管理下将提交变更; 如果当前不在事务的管理下则提交 flush;

### v2.5.1

Retrive(Object[] keys) 与 Retrive<TMember>(String field, params TMember[] keys) 开始从具体实现引入抽象类 Repository<TEntry>;

* NHibernate 以 ICriteria 特性实现;
* Mongo 以携带 Id 的 IMongoId<TKey> 为基础实现;

### v2.5.0

大幅简化了 NHibernate 相关实现, 移除了 IAggregate, 将 Id 的职责交还 NHibernate 的 Mapping 机制;