using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateRepository<TEntry> : Repository<TEntry> where TEntry : class {
        private readonly NHibernateRepositoryContext _context = null;

        public override IQueryable<TEntry> All {
            get {
                return _context.Of<TEntry>();
            }
        }

        public NHibernateRepositoryContext NHibernateRepositoryContext {
            get { return _context; }
        }

        public NHibernateRepository(IRepositoryContext context)
            : base(context) {
            _context = context as NHibernateRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateRepositoryContext but provided " + context.GetType().FullName);
            }
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            return SafeExecute(_ => query(_context.Of<TEntry>()));
        }

        private void SafeExecute(Action<ISession> action) {
            try {
                action(_context.EnsureSession());
            }
            catch (GenericADOException ex) {
                throw ex.InnerException;
            }
        }

        private TResult SafeExecute<TResult>(Func<ISession, TResult> func) {
            try {
                return func(_context.EnsureSession());
            }
            catch (GenericADOException ex) {
                throw ex.InnerException;
            }
        }

        public override TEntry Retrive(Object id) {
            return SafeExecute(session => session.Get<TEntry>(id));
        }

        public override IEnumerable<TEntry> Retrive(params object[] keys) {
            var metadata = _context.SessionFactory.GetClassMetadata(typeof(TEntry));
            return SafeExecute(session => {
                var criteria = session.CreateCriteria<TEntry>();
                criteria.Add(Restrictions.In(metadata.IdentifierPropertyName, keys));
                return criteria.List<TEntry>();
            });
        }

        //应使用实体属性名而非数据库列名
        public override IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys) {
            return SafeExecute(session => {
                var criteria = session.CreateCriteria<TEntry>();
                criteria.Add(Restrictions.In(field, keys));
                return criteria.List<TEntry>();
            });
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys) {
            return SafeExecute(_ => Retrive(ExpressionBuilder.GetPropertyInfo(selector).Name, keys));
        }

        public override void Create(TEntry entry) {
            SafeExecute(session => session.Save(entry));
        }

        public override void Update(TEntry entry) {
            SafeExecute(session => session.Update(entry));
        }

        public override void Save(TEntry entry) {
            SafeExecute(session => session.SaveOrUpdate(entry));
        }

        public override void Delete(TEntry entry) {
            SafeExecute(session => session.Delete(entry));
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            return SafeExecute(_ => {
                IQueryable<TEntry> query = All;
                foreach (var predicate in predicates) {
                    query = query.Where(predicate);
                }
                return query.Select(r => r).FirstOrDefault() != null;
            });
        }
    }
}
