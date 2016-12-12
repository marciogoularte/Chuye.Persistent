using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    public class DeferredPage<T> {
        private readonly IDatabase _db;
        private readonly PagedSql _pagedSql;
        private readonly Func<IList<T>, Object> _lastIdFunc;
        private List<T> _items;

        public IReadOnlyList<T> Items {
            get { return _items.AsReadOnly(); }
        }

        internal DeferredPage(IDatabase db, PagedSql pagedSql, Func<IList<T>, Object> lastIdFunc) {
            _db = db;
            _pagedSql = pagedSql;
            _lastIdFunc = lastIdFunc;
        }

        public Boolean Next(ref Object lastId) {
            _items = _db.Fetch<T>(_pagedSql.ToSql(lastId));
            if (_items != null && _items.Count > 0) {
                lastId = _lastIdFunc(_items);
                return true;
            }
            return false;
        }
    }
}
