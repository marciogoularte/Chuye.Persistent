using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.SqlCommand;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateInterceptor : EmptyInterceptor, IInterceptor {
#if DEBUG
        public override SqlString OnPrepareStatement(SqlString sql) {
            Debug.WriteLine(sql);
            return base.OnPrepareStatement(sql);
        }
#endif
    }
}
