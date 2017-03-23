// Owners: Karlov Nikolay

using System.Web;
using System.Web.Routing;
using QA.Core.Engine.Web.Mvc;
using QA.Core.Web;

namespace QA.Core.Engine
{
    
    public class ContentRoute<T> : ContentRoute, IRouteWithArea where T : AbstractItem
    {
        public ContentRoute(IEngine engine)
            : base(engine)
        {

            _controller = engine.Resolve<IControllerMapper>().GetControllerName(typeof(T));
        }

        public ContentRoute(IEngine engine, IRouteHandler routeHandler, IControllerMapper controllerMapper, Route innerRoute)
            : base(engine, routeHandler, controllerMapper, innerRoute)
        {
            _controller = engine.Resolve<IControllerMapper>().GetControllerName(typeof(T));

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
            else
            {
                SaveRouteData(rd, httpContext);
            }

            return null;
        }

        protected override RouteData SetRouteValues(AbstractItem item, PathData td)
        {
            var innerData = GetPath(item, td.Argument);
            if (innerData != null)
            {
                var data = new RouteData(this, _routeHandler);

                foreach (var defaultPair in innerData.Values)
                {
                    data.Values[defaultPair.Key] = defaultPair.Value;
                }

                return data;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {

            var item = values.CurrentItem<T>(AbstractItemKey, _engine.Persister)
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


        private RouteData GetPath(AbstractItem item, string remainingUrl)
        {
            if (remainingUrl == null)
                return null;
            var url = remainingUrl;
            var data = _innerRoute.GetRouteData(new CustomHttpContextWrapper(HttpContext.Current, url));

            if (data != null)
            {
                return data;
            }

            return null;
        }

        #region IRouteWithArea Members

        string area;
        private readonly string _controller;

        public string Area
        {
            get { return area; }
            protected set { area = value; }
        }

        #endregion
    }
}
