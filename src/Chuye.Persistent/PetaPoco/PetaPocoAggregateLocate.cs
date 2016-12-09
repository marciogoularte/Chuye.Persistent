using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    public class PetaPocoAggregateLocate : IAggregateLocate {
        private readonly PetaPocoUnitOfWork _unitOfWork;

        public PetaPocoAggregateLocate(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork as PetaPocoUnitOfWork;
            if (_unitOfWork == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect PetaPocoUnitOfWork but provided " + unitOfWork.GetType().FullName);
            }
        }

        public PetaPocoAggregateLocate(PetaPocoUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public TEntry FindById<TEntry>(Object key) {
            return _unitOfWork.Database.SingleOrDefault<TEntry>(key);
        }

        public void Save<TEntry>(TEntry entry) {
            _unitOfWork.Database.Save(entry);
        }

        public void Delete<TEntry>(TEntry entry) {
            _unitOfWork.Database.Delete(entry);
        }
    }
}
