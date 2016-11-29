﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;

namespace Chuye.Persistent.NHibernate {
    public class NHibernateUnitOfWork : IUnitOfWork {
        private Int32 _count = 0;
        private readonly Guid _id = Guid.NewGuid();
        private readonly NHibernateDbContext _context;
        private readonly NHibernateDbConfig _config;
        private ISession _session;
        private Boolean _suspendedTransaction;

        public Guid Id {
            get { return _id; }
        }

        internal Int32 Count {
            get { return _count; }
        }

        internal NHibernateDbContext Context {
            get { return _context; }
        }

        public NHibernateUnitOfWork(NHibernateDbContext context)
            : this(context, context.Config) {

        }

        public NHibernateUnitOfWork(NHibernateDbContext context, NHibernateDbConfig config) {
            _context = context;
            _config = config;
        }

        public ISession OpenSession() {
            EnsureSessionOpen();
            if (_config.Stragety.Require == TransactionRequire.Essential) {
                EnsureTransactionBegin();
            }
            else if (_config.Stragety.Require == TransactionRequire.Manual) {
                lock (_context) {
                    if (_suspendedTransaction) {
                        EnsureTransactionBegin();
                        _suspendedTransaction = false;
                    }
                }
            }
            return _session;
        }

        private void EnsureSessionOpen() {
            if (_session == null) {
                Debug.WriteLine("(NH:Session open, count {0})", Interlocked.Increment(ref _count));
                _session = _context.SessionFactory.OpenSession();
                if (_config.Stragety.Require == TransactionRequire.Manual) {
                    _session.FlushMode = FlushMode.Always;
                }
            }
        }

        private void EnsureTransactionBegin() {
            if (!_session.Transaction.IsActive) {
                Debug.WriteLine("(NH:Transaction begin)");
                _session.BeginTransaction();
            }
        }

        private IDisposable GetTransactionKeeper() {
            return new NHibernateTransactionKeeper(this);
        }

        private void EnsureTransactionRollback() {
            _session.Clear();
            if (_session.Transaction.IsActive) {
                Debug.WriteLine("(NH:Transaction rollback)");
                _session.Transaction.Rollback();
            }
        }

        public IDisposable Begin() {
            if (_config.Stragety.Time == TransactionTime.Immediately) {
                EnsureSessionOpen();
                EnsureTransactionBegin();
            }
            else {
                if (_session == null) {
                    _suspendedTransaction = true;
                }
                else {
                    EnsureTransactionBegin();
                }
            }
            return GetTransactionKeeper();
        }

        public void Rollback() {
            if (_session == null) {
                return;
            }
            EnsureTransactionRollback();
            if (_config.Stragety.Require == TransactionRequire.Essential) {
                EnsureTransactionBegin();
            }
        }

        //仅在事务已创建且处于活动中时提交事务
        public void Commit() {
            if (_session == null) {
                return;
            }
            EnsureTransactionCommit();
            if (_config.Stragety.Require == TransactionRequire.Essential) {
                EnsureTransactionBegin();
            }
        }

        private void EnsureTransactionCommit() {
            try {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction commit)");
                    _session.Transaction.Commit();
                }
            }
            catch {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction rollback)");
                    _session.Transaction.Rollback();
                }
                throw;
            }
            finally {
                if (_session.Transaction.IsActive) {
                    Debug.WriteLine("(NH:Transaction dispose)");
                    _session.Transaction.Dispose();
                }
            }
        }

        public void Dispose() {
            if (_session == null) {
                return;
            }
            try {
                if (_config.SaveUncommitted) {
                    if (_session.Transaction.IsActive) {
                        EnsureTransactionCommit();
                    }
                    else {
                        _session.Flush();
                    }
                }
                else {
                    EnsureTransactionRollback();

                }
            }
            finally {
                Debug.WriteLine("(NH:Session dispose, left {0})",
                    Interlocked.Decrement(ref _count));
                _session.Close();
                _session.Dispose();
                _session = null;
            }
        }
        
        internal class NHibernateTransactionKeeper : IDisposable {
            private readonly NHibernateUnitOfWork _unitOfWork;
            public NHibernateTransactionKeeper(NHibernateUnitOfWork unitOfWork) {
                _unitOfWork = unitOfWork;
            }

            public void Dispose() {
                if (_unitOfWork._config.SaveUncommitted) {
                    _unitOfWork.Commit();
                }
                else {
                    _unitOfWork.Rollback();
                }
            }
        }
    }
}
