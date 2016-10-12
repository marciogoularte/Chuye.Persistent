using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            : this(new Database(connectionStringName)) {
        }

        public PetaPocoUnitOfWork(String connectionString, String providerName)
            : this(new Database(connectionString, providerName)) {
        }

        public PetaPocoUnitOfWork(PetaPocoDbContext dbContext)
            : this(dbContext.OpenDatabase()) {
        }

        internal PetaPocoUnitOfWork(Database database) {
            if (database is InterceptedDatabase) {
                _database = database;
            }
            else {
                _database = database.Connection != null
                    ? new InterceptedDatabase(database.Connection)
                    : new InterceptedDatabase(database.ConnectionString, database.Provider.GetFactory());
            }
        }

        public IDisposable Begin() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Started, TransFlag.Initial) == TransFlag.Initial) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Begin: BeginTransaction", args: DateTime.Now);
                _database.BeginTransaction();
            }
            return new TransactionKeeper(this);
        }

        public void Commit() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Initial, TransFlag.Started) != TransFlag.Started) {
                return;
            }

            try {
                Debug.WriteLine("{0:HH:mm:ss.fff} Commit: CompleteTransaction", args: DateTime.Now);
                _database.CompleteTransaction();
            }
            catch (DbException) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Commit: AbortTransaction", args: DateTime.Now);
                Interlocked.Increment(ref _transactionFlag);
                _database.AbortTransaction();
                throw;
            }
        }

        public void Rollback() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Initial, TransFlag.Started) == TransFlag.Started) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Rollback: AbortTransaction", args: DateTime.Now);
                _database.AbortTransaction();
            }
        }

        public void Dispose() {
            if (Interlocked.CompareExchange(ref _transactionFlag,
                TransFlag.Initial, TransFlag.Started) == TransFlag.Started) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Dispose: AbortTransaction", args: DateTime.Now);
                _database.AbortTransaction();
            }

            _database.CloseSharedConnection();
            _database.Dispose();
        }


        static class TransFlag {
            public const Int32 Initial = 0;
            public const Int32 Started = 1;
        }

        class InterceptedDatabase : Database {
            private Exception _lastErrorOverTransaction;

            public Exception LastErrorOverTransaction {
                get { return _lastErrorOverTransaction; }
            }

            /*public ErrorCaredDatabase(String connectionStringName)
                : base(connectionStringName) {
            }

            public ErrorCaredDatabase(String connectionString, String providerName)
                : base(connectionString, providerName) {
            }

            public ErrorCaredDatabase(Database database)
                : base(database.ConnectionString, database.Provider.GetFactory()) {
            }*/

            public InterceptedDatabase(IDbConnection connection)
                : base(connection) {
            }

            public InterceptedDatabase(String connectionString, DbProviderFactory factory)
                : base(connectionString, factory) {
            }

            public override void OnBeginTransaction() {
                base.OnBeginTransaction();
                _lastErrorOverTransaction = null;
            }

            public override void OnExecutingCommand(IDbCommand cmd) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.fff} SQL: {1}", DateTime.Now, cmd.CommandText));
                base.OnExecutingCommand(cmd);
            }

            public override bool OnException(Exception ex) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.fff} OnException: {1}", DateTime.Now, ex.Message));
                _lastErrorOverTransaction = ex;
                return base.OnException(ex);
            }
        }

        class TransactionKeeper : IDisposable {
            private PetaPocoUnitOfWork _unitOfWork;

            public TransactionKeeper(PetaPocoUnitOfWork unitOfWork) {
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
