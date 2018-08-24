using System.Linq;
#pragma warning disable 1591

namespace QA.Core.Engine.Collections
{
    public interface IQueryableList<T>
    {
        IQueryable<T> Query();
    }
}
