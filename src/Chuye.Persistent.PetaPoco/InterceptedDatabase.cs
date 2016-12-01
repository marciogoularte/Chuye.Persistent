using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.PetaPoco {
    static class TransFlag {
        public const Int32 Initial = 0;
        public const Int32 Started = 1;
    }

    class InterceptedDatabase : Database {
        private Exception _lastErrorOverTransaction;

        public Exception LastErrorOverTransaction {
            get { return _lastErrorOverTransaction; }
        }

        public InterceptedDatabase(String connectionStringName)
            : base(connectionStringName) {
        }

        public InterceptedDatabase(String connectionString, String providerName)
            : base(connectionString, providerName) {
        }

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
            Debug.WriteLine("(PC:OnExecuting, sql {0})", args: cmd.CommandText);
            base.OnExecutingCommand(cmd);
        }

        public override bool OnException(Exception ex) {
            Debug.WriteLine("(PC:OnException, ex {0})", args: ex.Message);
            _lastErrorOverTransaction = ex;
            return base.OnException(ex);
        }
    }
}
