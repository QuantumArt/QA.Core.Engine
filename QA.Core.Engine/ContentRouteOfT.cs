// Owners: Karlov Nikolay

using System.Web;
using System.Web.Routing;
using QA.Core.Engine.Web.Mvc;

namespace QA.Core.Engine
{
    
    public class ContentRoute<T> : ContentRoute, IRouteWithArea where T : AbstractItem
    {
        IEngine engine;

        public ContentRoute(IEngine engine)
            : base(engine)
        {
            this.engine = engine;
        }

        public ContentRoute(IEngine engine, IRouteHandler routeHandler, IControllerMapper controllerMapper, Route innerRoute)
            : base(engine, routeHandler, controllerMapper, innerRoute)
        {
            this.engine = engine;
            if (innerRoute.DataTokens.ContainsKey("area"))
                this.Area = innerRoute.DataTokens["area"] as string;
        }

        public override RouteValueDictionary GetRouteValues(AbstractItem item, RouteValueDictionary routeValues)
        {
            if (item is T)
                return base.GetRouteValues(item, routeValues);
            return null;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var rd = base.GetRouteData(httpContext);

            if (rd != null && rd.CurrentItem() is T)
            {
                return rd;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {

            var item = values.CurrentItem<T>(AbstractItemKey, engine.Persister)
                ?? requestContext.CurrentItem<T>();
                      
            if (!(item is T))
                return null;

            values = new RouteValueDictionary(values);
            values[AreaKey] = Area;
            var vpd = base.GetVirtualPath(requestContext, values);

            if (vpd != null && Area != null)
                vpd.DataTokens[AreaKey] = Area;

            return vpd;
        }

        #region IRouteWithArea Members

        string area;
        public string Area
        {
            get { return area; }
            protected set { area = value; }
        }

        #endregion
    }
}
