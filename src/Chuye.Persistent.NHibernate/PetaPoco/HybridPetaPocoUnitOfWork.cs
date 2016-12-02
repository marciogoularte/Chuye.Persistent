using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Chuye.Persistent.PetaPoco;

namespace Chuye.Persistent.NHibernate.PetaPoco {
    public class HybridPetaPocoUnitOfWork : PetaPocoUnitOfWork {
        public HybridPetaPocoUnitOfWork(NHibernateUnitOfWork uow)
            : base(uow.OpenSession().Connection) {
        }
    }
}
