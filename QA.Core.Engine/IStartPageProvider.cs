
namespace QA.Core.Engine
{
    public interface IStartPageProvider
    {
        // AbstractItem GetStartPage();
        int GetStartPageId();
        int GetStartPageId(Url url);
        // AbstractItem GetRootPage();
        int GetRootPageId();
        AbstractItem GetRootPage();
        bool IsStartPage(int id);

        //string GetBinding(int id);
    }
}
