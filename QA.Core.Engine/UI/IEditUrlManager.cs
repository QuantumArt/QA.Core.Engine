#pragma warning disable 1591

namespace QA.Core.Engine.UI
{
    public interface IEditUrlManager
    {
        string GetEditExistingItemUrl(AbstractItem item);

        string GetDeleteUrl(AbstractItem item);

        string GetBaseNavigationUrl();

        string GetCreateUrl();
    }
}
