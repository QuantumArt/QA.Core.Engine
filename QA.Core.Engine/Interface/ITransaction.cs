using System;

namespace QA.Core.Engine
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
