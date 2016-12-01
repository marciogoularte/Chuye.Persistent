using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using PetaPoco;

namespace Chuye.Persistent.PetaPoco {
    public class PetaPocoUnitOfWork : IUnitOfWork {
        private readonly Guid _id = Guid.NewGuid();
        private Int32 _transactionFlag = 0;
        private readonly Database _database;

        public Guid ID {
            get { return _id; }
        }

        public Database Database {
            get { return _database; }
        }

        public PetaPocoUnitOfWork(String connectionStringName)
            : this(new InterceptedDatabase(connectionStringName)) {
        }

        public PetaPocoUnitOfWork(String connectionString, String providerName)
            : this(new InterceptedDatabase(connectionString, providerName)) {
        }

        public PetaPocoUnitOfWork(IDbConnection connection)
            : this(new InterceptedDatabase(connection)) {
        }

        internal PetaPocoUnitOfWork(InterceptedDatabase database) {
            Debug.WriteLine("(PC:Database ctor)");
            _database = database;
        }

        public IDisposable Begin() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Started, TransFlag.Initial) != TransFlag.Initial) {
                throw new InvalidOperationException();
            }

            Debug.WriteLine("(PC:Transaction begin)");
            _database.BeginTransaction();
            return new PetaPocoTransactionKeeper(this);
        }

        public void Commit() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Initial, TransFlag.Started) != TransFlag.Started) {
                throw new InvalidOperationException();
            }

            try {
                Debug.WriteLine("(PC:Transaction commit)");
                _database.CompleteTransaction();
            }
            catch (DbException) {
                Debug.WriteLine("(PC:Transaction rollback)");
                _database.AbortTransaction();
                throw;
            }
        }

        public void Rollback() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Initial, TransFlag.Started) != TransFlag.Started) {
                throw new InvalidOperationException();
            }
            Debug.WriteLine("(PC:Transaction rollback)");
            _database.AbortTransaction();
        }

        public void Dispose() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                 TransFlag.Initial, TransFlag.Started) == TransFlag.Started) {
                Debug.WriteLine("(PC:Transaction rollback)");
                _database.AbortTransaction();
            }

            Debug.WriteLine("(PC:Database dispose)");
            _database.CloseSharedConnection();
            _database.Dispose();
        }

        class PetaPocoTransactionKeeper : IDisposable {
            private PetaPocoUnitOfWork _unitOfWork;

            public PetaPocoTransactionKeeper(PetaPocoUnitOfWork unitOfWork) {
                if (!(unitOfWork.Database is InterceptedDatabase)) {
                    throw new ArgumentOutOfRangeException("unitOfWork.Database");
                }
                _unitOfWork = unitOfWork;
            }

            public void Dispose() {
                var db = (InterceptedDatabase)_unitOfWork.Database;
                if (db.LastErrorOverTransaction == null) {
                    _unitOfWork.Commit();
                }
                else {
                    _unitOfWork.Rollback();
                }
            }
        }
    }
}
