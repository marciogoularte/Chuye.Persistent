using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Chuye.Persistent.Repository;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateRepository<TEntry> : Repository<TEntry> where TEntry : class {
        private readonly NHibernateUnitOfWork _unitOfWork = null;

        public override IQueryable<TEntry> All {
            get {
                return _unitOfWork.OpenSession().Query<TEntry>();
            }
        }

        public NHibernateUnitOfWork UnitOfWork {
            get { return _unitOfWork; }
        }

        public NHibernateRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork) {
            _unitOfWork = unitOfWork as NHibernateUnitOfWork;
            if (_unitOfWork == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateUnitOfWork but provided " + unitOfWork.GetType().FullName);
            }
        }

        public override TEntry Retrive(Object id) {
            var session = _unitOfWork.OpenSession();
            return session.Get<TEntry>(id);
        }

        public override IEnumerable<TEntry> Retrive(params object[] keys) {
            var meta = _unitOfWork.Context.SessionFactory.GetClassMetadata(typeof(TEntry));
            var session = _unitOfWork.OpenSession();
            var criteria = session.CreateCriteria<TEntry>();
            criteria.Add(Restrictions.In(meta.IdentifierPropertyName, keys));
            return criteria.List<TEntry>();
        }

        /// <summary>
        /// NHibernate 会根据 field 获取元数据最终得到数据库中的 column
        /// 应使用实体属性名而非数据库列名
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="field"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys) {
            var session = _unitOfWork.OpenSession();
            var criteria = session.CreateCriteria<TEntry>();
            criteria.Add(Restrictions.In(field, keys));
            return criteria.List<TEntry>();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys) {
            return Retrive(ExpressionBuilder.GetPropertyInfo(selector).Name, keys);
        }

        public override void Create(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Save(entry);
        }

        public override void Update(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Update(entry);
        }

        public override void Save(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.SaveOrUpdate(entry);
        }

        public override void Delete(TEntry entry) {
            var session = _unitOfWork.OpenSession();
            session.Delete(entry);
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            var session = _unitOfWork.OpenSession();
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates) {
                query = query.Where(predicate);
            }
            return query.Select(r => r).FirstOrDefault() != null;
        }
    }
}
