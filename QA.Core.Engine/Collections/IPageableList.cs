using System.Linq;

namespace QA.Core.Engine.Collections
{
    public interface IPageableList<T>
    {
        IQueryable<T> FindRange(int skip, int take);
    }
}
