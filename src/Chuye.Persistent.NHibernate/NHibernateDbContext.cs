using System;
using NHibernate;
using NHibernate.Cfg;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateDbContext : IDisposable {
        private readonly Guid _id;
        private readonly NHibernateDbConfig _dbConfig;
        private Lazy<ISessionFactory> _sessionFactory;

        public Guid Id {
            get { return _id; }
        }

        internal NHibernateDbConfig DbConfig {
            get { return _dbConfig; }
        }

        public ISessionFactory SessionFactory {
            get {
                if (_sessionFactory == null) {
                    throw new InvalidProgramException();
                }
                return _sessionFactory.Value;
            }
        }

        public NHibernateDbContext()
            : this(NHibernateDbConfig.FromConfig("NHibernate:DbConfig")) {
        }

        public NHibernateDbContext(NHibernateDbConfig dbConfig) {
            _id = Guid.NewGuid();
            _dbConfig = dbConfig;
        }

        public NHibernateDbContext Setup(Func<Configuration> configFunc) {
            TryReleaseSessionFactory();
            _sessionFactory = new Lazy<ISessionFactory>(configFunc().BuildSessionFactory);
            return this;
        }

        private void TryReleaseSessionFactory() {
            if (_sessionFactory != null && _sessionFactory.IsValueCreated) {
                _sessionFactory.Value.Dispose();
            }
        }

        public void Dispose() {
            TryReleaseSessionFactory();
        }
    }
}
