using System.Linq;
#pragma warning disable 1591

namespace QA.Core.Engine.Collections
{
    public interface IPageableList<T>
    {
        IQueryable<T> FindRange(int skip, int take);
    }
}
