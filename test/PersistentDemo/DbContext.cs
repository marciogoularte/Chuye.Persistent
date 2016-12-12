using System;
using System.Configuration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Chuye.Persistent.NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace PersistentDemo {
    public class DbContext : NHibernateDbContext {
        private String _connectionString;

        public DbContext() {
            base.Setup(Configure);
        }

        public DbContext(String connectionString) {
            _connectionString = connectionString;
            base.Setup(Configure);
        }

        private NHibernate.Cfg.Configuration Configure() {
            var connectionString = _connectionString
                ?? ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            return Fluently.Configure()
                   .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DbContext>())
                   .ExposeConfiguration(conf => {
#if DEBUG
                       new SchemaExport(conf).Create(true, true);
#endif
                   })
                .BuildConfiguration()
                /*.SetInterceptor(new NHibernateInterceptor())*/
                .SetProperty(NHibernate.Cfg.Environment.ShowSql, Boolean.TrueString);

        }
    }
}