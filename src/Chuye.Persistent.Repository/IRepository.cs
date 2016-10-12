using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Repository {
    public interface IRepository<in TEntry, out TResult> {
        void Create(TEntry entry);
        void Update(TEntry entry);
        void Delete(TEntry entry);
        void Save(TEntry entry);

        IQueryable<TResult> All { get; }
        TResult Retrive(Object id);
        IEnumerable<TResult> Retrive(Object[] keys);
        IEnumerable<TResult> Retrive<TKey>(String field, params TKey[] keys);
    }
}
