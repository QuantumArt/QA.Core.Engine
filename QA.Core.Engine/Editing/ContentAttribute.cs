using System;
using System.Diagnostics;
using System.Web.Mvc;
using QA.Core.Engine.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Editing
{
    public class ContentAttribute : ActionFilterAttribute
    {
        public const string UrlQueryStringKey = "targetUrl";
        public const string ItemQueryStringKey = "targetId";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //TODO: получить откуда-нибудь еще!!!!
            const string AppBaseUrl = "/";

            var nestedUrl = new Url(filterContext.RequestContext.HttpContext.Request.QueryString[UrlQueryStringKey] ?? AppBaseUrl);
            var parser = ObjectFactoryBase.Resolve<IUrlParser>();
            var url = new Url(filterContext.RequestContext.HttpContext.Request.Url.ToString());

            AbstractItem currentPage = null;
            AbstractItem currentItem = null;

            if (nestedUrl.ToString().ToLower().Contains("cms/managment"))
            {
                filterContext.Result = new RedirectResult(nestedUrl.ToString());
            }

            try
            {
                var data = parser.Parse(nestedUrl);

                if (data != null)
                {
                    currentPage = data;
                    currentItem = data;

                    filterContext.RouteData.Values.Add(ContentRoute.AbstractItemKey, currentItem);
                    filterContext.RouteData.Values.Add(ContentRoute.ContentPageKey, currentItem);
                    filterContext.RouteData.Values.Add(ContentRoute.RootPageKey, parser.RootPage);
                    filterContext.RouteData.Values.Add(ContentRoute.StartPageKey, parser.StartPage);
                    filterContext.RouteData.Values.Add("PathData", data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            filterContext.RouteData.Values.Add("NestedUrl", nestedUrl);

            base.OnActionExecuting(filterContext);
        }
    }
}
