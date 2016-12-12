using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    public static class DatabaseExtension {

        public static DeferredPage<T> DeferPage<T>(this IDatabase db, String pageSql, Func<IList<T>, Object> lastIdFunc) {
            return new DeferredPage<T>(db, new PagedSql(pageSql), lastIdFunc);
        }

        public static DeferredPage<T> DeferPage<T>(this IDatabase db, Sql pageSql, Func<IList<T>, Object> lastIdFunc) {
            return new DeferredPage<T>(db, new PagedSql(pageSql), lastIdFunc);
        }
    }
}
