using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
    /// <summary>
    /// Widget platform UrlHelper extensions
    /// </summary>
    public static class UrlExtensions
    {
        public static string ActionToItem(this UrlHelper urlHelper, AbstractItem item, string action, object routeValues)
        {
            return urlHelper.ActionToItem(item, action, new RouteValueDictionary(routeValues));
        }

        public static string ActionToItem(this UrlHelper urlHelper, AbstractItem item, string action, RouteValueDictionary routeValues)
        {
            var values = new RouteValueDictionary(routeValues);

            values["action"] = action;
            values[ContentRoute.AbstractItemKey] = item;

            return urlHelper.Action(action, values);
        }

        /// <summary>
        /// Build url to action of particular page or widget
        /// </summary>
        /// <param name="urlHelper">helper</param>
        /// <param name="itemId">Page or widget id</param>
        /// <param name="action">action name</param>
        /// <param name="values">route values</param>
        /// <returns></returns>
        public static string ActionToItem(this UrlHelper urlHelper, int itemId, string action, object values)
        {
            var routeValues = new RouteValueDictionary(values);
            return ActionToItem(urlHelper, itemId, action, routeValues);
        }

        /// <summary>
        /// Build url to action of particular page or widget
        /// </summary>
        /// <param name="urlHelper">helper</param>
        /// <param name="itemId">Page or widget id</param>
        /// <param name="action">action name</param>
        /// <param name="routeValues">route values</param>
        /// <returns></returns>
        public static string ActionToItem(UrlHelper urlHelper, int itemId, string action, RouteValueDictionary routeValues)
        {
            var engine = urlHelper.RequestContext.RouteData.GetEngine();

            if (engine == null)
                throw new InvalidOperationException("Engine is null");

            var item = engine.Persister.Get(itemId);

            if (item != null)
            {
                return urlHelper.ActionToItem(item, action, routeValues);
            }

            return string.Empty;
        }
    }
}
