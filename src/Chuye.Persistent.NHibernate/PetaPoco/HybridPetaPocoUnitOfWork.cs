using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Chuye.Persistent.NHibernate;

namespace Chuye.Persistent.PetaPoco {
    public class HybridPetaPocoUnitOfWork : PetaPocoUnitOfWork {
        public HybridPetaPocoUnitOfWork(NHibernateUnitOfWork uow)
            : base(uow.OpenSession().Connection) {
        }
    }
}
