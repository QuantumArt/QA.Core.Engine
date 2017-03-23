using System;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
    public static class RouteExtensions
    {
        public static IEngine GetEngine(this RouteData routeData)
        {
            return routeData.DataTokens[ContentRoute.ContentEngineKey] as IEngine
                ?? ObjectFactoryBase.Resolve<IEngine>();
        }

        public static T ResolveService<T>(this RouteData routeData) where T : class
        {
            return GetEngine(routeData).Resolve<T>();
        }

        public static RouteData ApplyCurrentItem(this RouteData data, AbstractItem page, AbstractItem part)
        {
            return data.ApplyAbstractItem(ContentRoute.ContentPageKey, page)
                .ApplyAbstractItem(ContentRoute.ContentPartKey, part);
        }

        public static RouteData ApplyCurrentItem(this RouteData data, string controllerName, string actionName, AbstractItem page, AbstractItem part)
        {
            data.Values[ContentRoute.ControllerKey] = controllerName;
            data.Values[ContentRoute.ActionKey] = actionName;
            return data.ApplyCurrentItem(page, part);
        }

        internal static RouteData ApplyAbstractItem(this RouteData data, string key, AbstractItem item)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (key == null) throw new ArgumentNullException("key");

            if (item != null)
            {
                data.DataTokens[key] = item;
                data.Values[key] = item.Id;
            }

            return data;
        }

        public static AbstractItem CurrentItem(this RouteData routeData)
        {
            return routeData.DataTokens.CurrentItem(ContentRoute.ContentPartKey)
                ?? routeData.DataTokens.CurrentItem(ContentRoute.ContentPageKey);
        }

        public static AbstractItem CurrentPage(this RouteData routeData)
        {
            return routeData.DataTokens.CurrentItem(ContentRoute.ContentPageKey);
        }

        internal static AbstractItem CurrentItem(this RouteValueDictionary data, string key)
        {
            if (data.ContainsKey(key))
                return data[key] as AbstractItem;
            return null;
        }

        public static T CurrentItem<T>(this RouteValueDictionary data, string key, IPersister persister)
            where T : AbstractItem
        {
            if (data.ContainsKey(key))
            {
                object value = data[key];
                if (value == null)
                    return null;

                var item = value as T;
                if (item != null)
                    return item as T;

                if (value is int)
                {
                    item = persister.Get((int)value) as T;
                    data[key] = item;
                    return item;
                }

                int itemId;
                if (value is string && int.TryParse(value as string, out itemId))
                    return persister.Get((int)value) as T;
            }

            return null;
        }
    }
}
