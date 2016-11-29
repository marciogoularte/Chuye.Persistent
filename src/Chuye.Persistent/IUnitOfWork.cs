using System;

namespace Chuye.Persistent {
    public interface IUnitOfWork : IDisposable {
        IDisposable Begin();
        void Clear();
        void Commit();
        void Rollback();
    }
}