using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PetaPoco;

namespace Chuye.Persistent.PetaPoco {
    public static class DatabaseExtension {

        public static DeferredPage<T> DeferPage<T>(this IDatabase db, String sql, Func<IList<T>, Object> lastIdFunc) {
            return DeferPage<T>(db, new PagedSql(sql), lastIdFunc);
        }

        public static DeferredPage<T> DeferPage<T>(this IDatabase db, Sql sql, Func<IList<T>, Object> lastIdFunc) {
            return DeferPage<T>(db, new PagedSql(sql), lastIdFunc);
        }

        internal static DeferredPage<T> DeferPage<T>(this IDatabase db, PagedSql pagedSql, Func<IList<T>, Object> lastIdFunc) {
            return new DeferredPage<T>(db, pagedSql, lastIdFunc);
        }
    }
}
