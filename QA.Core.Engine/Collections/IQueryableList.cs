using System.Linq;

namespace QA.Core.Engine.Collections
{
    public interface IQueryableList<T>
    {
        IQueryable<T> Query();
    }
}
