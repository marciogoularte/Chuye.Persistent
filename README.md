## Release log

### v2.5.2

�˰汾��nhibernate.session ����ʱ��δ�ύ�ı��Ĭ�Ͻ�������������ͨ����ѡ����������ļ��޸Ĵ���Ϊ

```xml
  <configSections>
    <section name="nhibernateBehaviour" type="Chuye.Persistent.NHibernate.NhibernateBehaviourConfigurationSection, Chuye.Persistent.NHibernate" />
  </configSections>
  <nhibernateBehaviour alwaysCommit="false" />
```

* alwaysCommit="false": Ĭ����Ϊ, ����δ�ύ�����������;
* alwaysCommit="true":  �����ǰ session ����������½��ύ���; �����ǰ��������Ĺ��������ύ flush;

### v2.5.1

Retrive(Object[] keys) �� Retrive<TMember>(String field, params TMember[] keys) ��ʼ�Ӿ���ʵ����������� Repository<TEntry>;

* NHibernate �� ICriteria ����ʵ��;
* Mongo ��Я�� Id �� IMongoId<TKey> Ϊ����ʵ��;

### v2.5.0

������� NHibernate ���ʵ��, �Ƴ��� IAggregate, �� Id ��ְ�𽻻� NHibernate �� Mapping ����;