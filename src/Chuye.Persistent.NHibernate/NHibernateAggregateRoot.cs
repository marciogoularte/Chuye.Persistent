using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateAggregateRoot : IAggregateRoot {
        private readonly NHibernateUnitOfWork _unitOfWork;

        public NHibernateAggregateRoot(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork as NHibernateUnitOfWork;
            if (_unitOfWork == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateUnitOfWork but provided " + unitOfWork.GetType().FullName);
            }
        }

        public NHibernateAggregateRoot(NHibernateUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public TEntry RetriveByKey<TEntry>(Object key) {
            var session = _unitOfWork.OpenSession();
            return session.Get<TEntry>(key);
        }

        public void Save<TEntry>(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.SaveOrUpdate(entry);
        }

        public void Delete<TEntry>(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Delete(entry);
        }
    }
}
