using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateRepository {
        protected NHibernateUnitOfWork Context { get; private set; }

        public NHibernateRepository(IUnitOfWork context) {
            if (!(context is NHibernateUnitOfWork)) {
                throw new ArgumentOutOfRangeException("context");
            }
            Context = (NHibernateUnitOfWork)context;
        }
    }
}
