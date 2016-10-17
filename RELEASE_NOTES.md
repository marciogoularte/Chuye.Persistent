
### 2.6

* 移除了 MongoDb 相关的实现, 将 Repository 作为可选实现;
* 将 UnitOfWork 职责从 NHibernateDbContext 剥离到 NHibernateUnitOfWork 作为更轻更核心的实现;
* 添加基于 PetaPoco 的 UnitOfWork 实现

### v2.5

* Nhibernate.Session 结束时未提交的变更的处置方式依赖于 NHibernate 行为策略，可以通过配置修改默认行为;
* 大幅简化了 NHibernate 相关实现, 移除了 IAggregate, 将 Id 的职责交还 NHibernate 的 Mapping 机制; 
* 泛型 Repository 只在 Mongo 实现中保留;