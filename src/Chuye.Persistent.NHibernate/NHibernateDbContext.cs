using System;
using NHibernate;
using NHibernate.Cfg;

namespace Chuye.Persistent.NHibernate {
    public abstract class NHibernateDbContext : IDisposable {
        private readonly Guid _id;
        private Boolean _alwaysCommit;
        private ISessionFactory _sessionFactory;

        public Guid ID {
            get { return _id; }
        }

        internal virtual Boolean AlwaysCommit {
            get { return _alwaysCommit; }
        }

        internal ISessionFactory SessionFactory {
            get { return _sessionFactory; }
        }

        public NHibernateDbContext() {
            _id = Guid.NewGuid(); ;
            var alwaysCommit = System.Configuration.ConfigurationManager.AppSettings
                .Get("NHibernate:alwaysCommit");
            _alwaysCommit = Boolean.TrueString.Equals(alwaysCommit, StringComparison.OrdinalIgnoreCase);
        }

        protected void SetupConfiguration(Func<Configuration> func) {
            _sessionFactory = func().BuildSessionFactory();
        }

        public void Dispose() {
            _sessionFactory.Dispose();
        }
    }
}
