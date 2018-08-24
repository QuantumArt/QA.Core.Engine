using System;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
