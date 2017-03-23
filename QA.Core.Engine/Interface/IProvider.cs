using System.Collections.Generic;

namespace QA.Core.Engine
{
    public interface IProvider<T>
    {
        T Get();
        IEnumerable<T> GetAll();
    }
}
