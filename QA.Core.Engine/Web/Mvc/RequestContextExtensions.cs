
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
    /// <summary>
    /// Расширения для получения информации о страницах и виджетах
    /// </summary>
    public static class RequestContextExtensions
    {
        /// <summary>
        /// Преобразовать в строку запроса
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ToQueryString<K, V>(this IDictionary<K, V> values)
        {
            if (values == null)
                return null;
            return string.Join("&", values.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());
        }

        /// <summary>
        /// Получение текущих страницы или виджета
        /// </summary>
        public static AbstractItem CurrentItem(this RequestContext context)
        {
            return context.CurrentItem<AbstractItem>();
        }

        /// <summary>
        /// Получение текущего виджета
        /// </summary>
        public static T CurrentPart<T>(this RequestContext context) where T : AbstractItem
        {
            return context.CurrentItem<T>(ContentRoute.ContentPartKey);
        }

        /// <summary>
        /// Получение текущих страницы или виджета
        /// </summary>
        public static T CurrentItem<T>(this RequestContext context) where T : AbstractItem
        {
            return context.CurrentItem<T>(ContentRoute.AbstractItemKey)
                ?? context.CurrentItem<T>(ContentRoute.ContentPartKey)
                ?? context.CurrentItem<T>(ContentRoute.ContentPageKey);
        }

        /// <summary>
        /// Получение текущей страницы
        /// </summary>
        public static T CurrentPage<T>(this RequestContext context) where T : AbstractItem
        {
            return context.CurrentItem<T>(ContentRoute.ContentPageKey);
        }

        /// <summary>
        /// Получение текущей страницы
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static AbstractItem CurrentPage(this RequestContext context)
        {
            return context.CurrentItem<AbstractItem>(ContentRoute.ContentPageKey);
        }

        /// <summary>
        /// Стартовая страница
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static AbstractItem StartPage(this RequestContext context)
        {
            return context.RouteData
                .ResolveService<IUrlParser>()
                .StartPage;
        }

        internal static T CurrentItem<T>(this RequestContext context, string key) where T : AbstractItem
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.RouteData.DataTokens[key] as T
                ?? context.RouteData.Values.CurrentItem<T>(key, RouteExtensions.GetEngine(context.RouteData).Persister);
        }
    }
}
