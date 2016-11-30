using System;
using NHibernate;
using NHibernate.Cfg;

namespace Chuye.Persistent.NHibernate {
    public abstract class NHibernateDbContext : IDisposable {
        private readonly Guid _id;
        private NHibernateDbConfig _config;
        private ISessionFactory _sessionFactory;

        public Guid Id {
            get { return _id; }
        }

        internal NHibernateDbConfig Config {
            get { return _config; }
        }

        internal ISessionFactory SessionFactory {
            get { return _sessionFactory; }
        }

        public NHibernateDbContext() {
            _id = Guid.NewGuid(); 
            _config = NHibernateDbConfig.FromConfig("NHibernate:DbConfig");
        }

        protected void SetupConfiguration(Func<Configuration> func) {
            _sessionFactory = func().BuildSessionFactory();
        }

        public void Dispose() {
            _sessionFactory.Dispose();
        }
    }
}
