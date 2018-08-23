using System.Web.Mvc;
using QA.Core.Engine;
using QA.Core.Engine.Web;
using QA.Core.Engine.Web.Mvc;
#pragma warning disable 1591

namespace QA.Engine.Extensions.Html
{
    public static class MenuHtmlExtensions
    {
        public static MenuHtmlHelper Menu(this HtmlHelper helper)
        {
            return new MenuHtmlHelper(helper, (AbstractItem)helper.ViewContext.ViewData[ContentRoute.AbstractItemKey]
                ??
                //HARDCODE!!!!!!!
                (AbstractItem)helper.ViewContext.ViewData["CurrentPage"],
                (AbstractItem)helper.ViewContext.ViewData[ContentRoute.StartPageKey] ??
                RouteExtensions.ResolveService<IUrlParser>(helper.ViewContext.RouteData).StartPage);
        }

        public static MenuHtmlHelper Tree(this HtmlHelper helper)
        {
            return new MenuHtmlHelper(helper, (AbstractItem)helper.ViewContext.ViewData[ContentRoute.AbstractItemKey]
                ??
                //HARDCODE!!!!!!!
                (AbstractItem)helper.ViewContext.ViewData["CurrentPage"],
                (AbstractItem)helper.ViewContext.RouteData.Values[ContentRoute.RootPageKey] ??
                RouteExtensions.ResolveService<IUrlParser>(helper.ViewContext.RouteData).RootPage);
        }
    }
}
