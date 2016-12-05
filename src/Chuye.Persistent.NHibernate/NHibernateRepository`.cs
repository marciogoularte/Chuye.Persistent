using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateRepository<TEntry> : NHibernateIRepository<TEntry> where TEntry : class {
        private readonly NHibernateUnitOfWork _unitOfWork = null;

        public IQueryable<TEntry> All {
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

        public TEntry FindById(Object id) {
            var session = _unitOfWork.OpenSession();
            return session.Get<TEntry>(id);
        }

        public IEnumerable<TEntry> FindByKeys(params Object[] keys) {
            var meta = _unitOfWork.Context.SessionFactory.GetClassMetadata(typeof(TEntry));
            var session = _unitOfWork.OpenSession();
            var criteria = session.CreateCriteria<TEntry>();
            criteria.Add(Restrictions.In(meta.IdentifierPropertyName, keys));
            return criteria.List<TEntry>();
        }
        

        public void Save(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.SaveOrUpdate(entry);
        }

        public void Delete(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Delete(entry);
        }
    }
}
