using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Dapper;
using System.Threading;
using System.Data;
using System.Diagnostics;

namespace Chuye.Persistent.Dapper {
    public class DapperUnitOfWork : IUnitOfWork {
        private IDbTransaction _trans;
        private readonly IDbConnection _connection;
        private Int32 opend;

        public IDbConnection Connection {
            get { return _connection; }
        }

        public DapperUnitOfWork(IDbConnection connection) {
            _connection = connection;
        }

        [Obsolete("不能实现'未出现异常时提交事务'的能力")]
        public IDisposable Begin() {
            if (Interlocked.CompareExchange(ref opend, 1, 0) == 0) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Begin: Open", args: DateTime.Now);
                _connection.Open();
            }
            if (Interlocked.CompareExchange(ref opend, 2, 1) == 1) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Begin: BeginTransaction", args: DateTime.Now);
                _trans = _connection.BeginTransaction();
            }
            return _trans;
        }

        public void Commit() {
            if (Interlocked.CompareExchange(ref opend, 1, 2) != 2) {
                return;
            }
            try {
                Debug.WriteLine("{0:HH:mm:ss.fff} Commit: Commit", args: DateTime.Now);
                _trans.Commit();
            }
            catch (DbException) {
                Interlocked.Increment(ref opend);
                Debug.WriteLine("{0:HH:mm:ss.fff} Commit: Rollback", args: DateTime.Now);
                _trans.Rollback();
                throw;
            }
            finally {
                _trans.Dispose();
                _trans = null;
            }
        }

        public void Rollback() {
            if (Interlocked.CompareExchange(ref opend, 1, 2) != 2) {
                return;
            }
            Debug.WriteLine("{0:HH:mm:ss.fff} Rollback: Rollback", args: DateTime.Now);
            _trans.Rollback();
            _trans.Dispose();
            _trans = null;
        }

        public void Dispose() {
            if (_trans != null) {
                Debug.WriteLine("{0:HH:mm:ss.fff} Dispose: Dispose", args: DateTime.Now);
                _trans.Rollback();
                _trans.Dispose();
            }
            _connection.Dispose();
        }
    }
}
