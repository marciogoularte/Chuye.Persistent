using System;

namespace Chuye.Persistent {
    public interface IUnitOfWork : IDisposable {
        IDisposable Begin();
        void Commit();
        void Rollback();
    }
}