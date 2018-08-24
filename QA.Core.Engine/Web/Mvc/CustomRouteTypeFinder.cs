using QA.Core.Web;
using System.Web;
using System.Web.Routing;
#pragma warning disable 1591

namespace QA.Core.Engine.Web.Mvc
{
    public class CustomRouteTypeFinder : IPathFinder
    {
        private readonly string _controller;
        private readonly Route _innerRoute;

        public CustomRouteTypeFinder(IControllerMapper controllerMapper, string controller, Route innerRoute)
        {
            _innerRoute = innerRoute;
            _controller = controller;
        }

        public PathData GetPath(AbstractItem item, string remainingUrl)
        {
            var url = string.Format("{0}/{1}", _controller, remainingUrl.TrimStart('/'));
            var data = _innerRoute.GetRouteData(new CustomHttpContextWrapper(HttpContext.Current, url));

            if (data != null)
            {
                return new PathData(item, url, data.Values["action"].ToString(), url);
            }

            return null;
        }
    }
}
