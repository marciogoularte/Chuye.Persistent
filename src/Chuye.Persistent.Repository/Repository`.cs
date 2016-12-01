using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Repository {
    public abstract class Repository<TEntry> : IRepository<TEntry> {
        public abstract IQueryable<TEntry> All { get; }
        public abstract Boolean Any(params Expression<Func<TEntry, Boolean>>[] predicates);
        public abstract TEntry Retrive(Object id);
        public abstract IEnumerable<TEntry> Retrive(Object[] keys);
        //public abstract IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys);
        public abstract IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys);

        public abstract void Create(TEntry entry);
        public abstract void Update(TEntry entry);
        public abstract void Delete(TEntry entry);
        public abstract void Save(TEntry entry);
    }
}
