using System.Web.Routing;

namespace QA.Core.Engine
{
    /// <summary>
    /// Роут, генерирующий адреса с маленькими буквами
    /// </summary>
    public class LowercaseRoute : Route
    {
        #region Constructors
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url"></param>
        /// <param name="routeHandler"></param>
        public LowercaseRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler) { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        /// <param name="routeHandler"></param>
        public LowercaseRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler) { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        /// <param name="constraints"></param>
        /// <param name="routeHandler"></param>
        public LowercaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler) { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        /// <param name="constraints"></param>
        /// <param name="dataTokens"></param>
        /// <param name="routeHandler"></param>
        public LowercaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler) { }

        #endregion

        /// <summary>
        /// Provides properties and methods for defining a route and for obtaining information
        /// about the route.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var vpd = base.GetVirtualPath(requestContext, values);

            if (vpd != null)
            {
	            Url url = vpd.VirtualPath;

				vpd.VirtualPath = url.SetPath(url.Path.ToLower());
            }

            return vpd;
        }
    }
}
