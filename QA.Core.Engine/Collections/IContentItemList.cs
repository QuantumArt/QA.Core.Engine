
namespace QA.Core.Engine.Collections
{
    public interface IAbstractItemList<T> : IContentList<T>, IZonedList<T>
        where T : AbstractItem
    {
    }
}
