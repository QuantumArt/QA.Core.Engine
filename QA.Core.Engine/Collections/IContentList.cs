using System.Collections.Generic;

namespace QA.Core.Engine.Collections
{
    public interface IContentList<T>:  IList<T>, INamedList<T>, IPageableList<T>, IQueryableList<T> where T : class, INameable
    {
    }
}
