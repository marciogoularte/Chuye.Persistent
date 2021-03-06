﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateRepository<TEntry> :INHibernateRepository<TEntry> where TEntry : class {
        private readonly NHibernateUnitOfWork _unitOfWork = null;

        public virtual IQueryable<TEntry> All {
            get {
                return _unitOfWork.OpenSession().Query<TEntry>();
            }
        }

        public NHibernateUnitOfWork UnitOfWork {
            get { return _unitOfWork; }
        }

        public NHibernateRepository(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork as NHibernateUnitOfWork;
            if (_unitOfWork == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateUnitOfWork but provided " + unitOfWork.GetType().FullName);
            }
        }

        public virtual TEntry RetriveByKey(Object key) {
            var session = _unitOfWork.OpenSession();
            return session.Get<TEntry>(key);
        }

        public virtual IEnumerable<TEntry> RetriveByKeys(params Object[] keys) {
            var session = _unitOfWork.OpenSession();
            var criteria = session.CreateCriteria<TEntry>();
            var metadata = _unitOfWork.Context.SessionFactory.GetClassMetadata(typeof(TEntry));
            if (metadata == null || metadata.IdentifierPropertyName == null) {
                throw new ArgumentOutOfRangeException("TEntry", 
                    String.Format("Mapping for {0} failed", typeof(TEntry).FullName));
            }
            criteria.Add(Restrictions.In(metadata.IdentifierPropertyName, keys));
            return criteria.List<TEntry>();
        }


        public virtual void Save(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.SaveOrUpdate(entry);
        }

        public virtual void Delete(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Delete(entry);
        }
    }
}
