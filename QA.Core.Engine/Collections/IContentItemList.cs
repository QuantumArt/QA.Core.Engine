
#pragma warning disable 1591

namespace QA.Core.Engine.Collections
{
    public interface IAbstractItemList<T> : IContentList<T>, IZonedList<T>
        where T : AbstractItem
    {
    }
}
