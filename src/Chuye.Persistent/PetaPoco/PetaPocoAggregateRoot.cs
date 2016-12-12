using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    public class PetaPocoAggregateRoot : IAggregateRoot {
        private readonly PetaPocoUnitOfWork _unitOfWork;

        public PetaPocoAggregateRoot(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork as PetaPocoUnitOfWork;
            if (_unitOfWork == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect PetaPocoUnitOfWork but provided " + unitOfWork.GetType().FullName);
            }
        }

        public PetaPocoAggregateRoot(PetaPocoUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public TEntry RetriveByKey<TEntry>(Object key) {
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
