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
        TResult RetriveByKey(Object id);
        IEnumerable<TResult> RetriveByKeys(params Object[] keys);
    }
}
