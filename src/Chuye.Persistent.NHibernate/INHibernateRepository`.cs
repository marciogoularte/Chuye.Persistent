using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NHibernate {
    public interface INHibernateRepository<in TEntry, out TResult> {
        void Delete(TEntry entry);
        void Save(TEntry entry);

        IQueryable<TResult> All { get; }
        TResult FindById(Object id);
        IEnumerable<TResult> FindByKeys(Object[] keys);
    }
}
