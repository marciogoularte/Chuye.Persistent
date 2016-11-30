using System;
using System.Configuration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using Chuye.Persistent;
using Chuye.Persistent.NHibernate;

namespace PersistentDemo {
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
                   .ExposeConfiguration(conf => {
#if DEBUG
                       if (Boolean.TrueString.Equals(ConfigurationManager.AppSettings.Get("NHibernate:recreate"), StringComparison.OrdinalIgnoreCase)) {
                           //new SchemaExport(conf).Create(true, true);
                       }
#endif
                   })
                .BuildConfiguration()
                /*.SetInterceptor(new NHibernateInterceptor())
                .SetProperty(NHibernate.Cfg.Environment.ShowSql, Boolean.TrueString)*/;

        }
    }
}