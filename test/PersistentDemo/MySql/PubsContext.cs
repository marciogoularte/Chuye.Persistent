﻿using System;
using System.Linq;
using Chuye.Persistent.NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Event;
using NHibernate.Event.Default;

namespace PersistentDemo.MySql {
    public class PubsContext : NHibernateRepositoryContext {
        private static readonly ISessionFactory _dbFactory;
        private static readonly EventDispatcher _eventDispatcher;

        public IEventDispatcher EventDispatcher {
            get { return _eventDispatcher; }
        }

        public static ISessionFactory DbFactory {
            get { return _dbFactory; }
        }

        static PubsContext() {
            _eventDispatcher = new EventDispatcher();
            _dbFactory = BuildSessionFactory();
        }

        public PubsContext()
            : base(_dbFactory) {
        }

        private static ISessionFactory BuildSessionFactory() {
            var dbConStr = System.Configuration.ConfigurationManager.ConnectionStrings["PubsMysql"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MySQLConfiguration.Standard.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PubsContext>())
                   .ExposeConfiguration(conf => {
                       //conf.SetInterceptor(new NHibernateInterceptor());
                       conf.SetProperty(NHibernate.Cfg.Environment.ShowSql, Boolean.TrueString);
                   });
            var dbConfig = dbFluentConfig.BuildConfiguration();

            ////尝试添加 PostLoadEventListener
            //var listeners = dbConfig.EventListeners.PostLoadEventListeners.ToList();
            //listeners.Add(_eventDispatcher.PostLoadEventListener);
            //dbConfig.EventListeners.PostLoadEventListeners = listeners.ToArray();
            return dbConfig.BuildSessionFactory();
        }
    }

    public interface IEventDispatcher {
        event EventHandler<PostLoadEvent> PostLoad;
    }

    class EventDispatcher : IEventDispatcher {
        public event EventHandler<PostLoadEvent> PostLoad;

        public readonly IPostLoadEventListener PostLoadEventListener;

        public EventDispatcher() {
            PostLoadEventListener = new CustomPostLoadEventListener(this);
        }

        private void OnPostLoad(PostLoadEvent @event) {
            if (PostLoad != null) {
                PostLoad(this, @event);
            }
        }

        internal class CustomPostLoadEventListener : DefaultPostLoadEventListener {
            private readonly EventDispatcher _eventDispatcher;
            public CustomPostLoadEventListener(EventDispatcher eventDispatcher) {
                _eventDispatcher = eventDispatcher;
            }

            public override void OnPostLoad(PostLoadEvent @event) {
                base.OnPostLoad(@event);
                _eventDispatcher.OnPostLoad(@event);
            }
        }
    }
}
