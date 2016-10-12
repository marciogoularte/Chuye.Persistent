using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PetaPoco;

namespace Chuye.Persistent.PetaPoco {
    internal class PagedSql {
        private const String PagePt = @"limit\s+\d+";
        public String Sql { get; private set; }

        public PagedSql(Sql sql) {
            Sql = sql.SQL;
        }

        public PagedSql(String sql) {
            if (!Regex.IsMatch(sql, PagePt)) {
                throw new ArgumentOutOfRangeException("limit", "Sql limit missing");
            }
            Sql = sql;
        }

        public Sql ToSql(Object lastId) {
            return new Sql(Sql, lastId);
        }
    }
}
