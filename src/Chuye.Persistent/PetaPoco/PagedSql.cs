using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    internal class PagedSql {
        private const String PagePt = @"WHERE\s+(?<w>.+)\s+ORDER\s+BY\s+(?<o>.+?)\s+(?:(?:ASC)|(?:DESC)\s+)?LIMIT\s+(?<l>\d+(?:,\d+)?)";
        public String SqlString { get; private set; }

        public PagedSql(Sql sql)
            : this(sql.SQL) {
        }

        public PagedSql(String sql) {
            if (String.IsNullOrWhiteSpace(sql)) {
                throw new ArgumentOutOfRangeException("sql", "Sql null or empty");
            }
            var match = Regex.Match(sql, PagePt, RegexOptions.IgnoreCase);
            if (!match.Success) {
                throw new ArgumentOutOfRangeException("sql", "Regex match failed, 'where' or 'limit', 'order by' clause may missing");
            }
            SqlString = sql;
        }

        public Sql ToSql(Object lastId) {
            return new Sql(SqlString, lastId);
        }

        public static implicit operator PagedSql(String sql) {
            return new PagedSql(sql);
        }
    }
}
