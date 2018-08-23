using System.Collections.Generic;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    public interface IProvider<T>
    {
        T Get();
        IEnumerable<T> GetAll();
    }
}
