using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Web.Mvc;
using System;

namespace QA.Core.Engine.Web.Mvc
{
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Устанавливает маршрут страниц и виджетов.
        /// т.е адреса вида: company\...\about\jobs\programmer
        /// HtmlPage?page=1234&part=12345
        /// </summary>
        /// <param name="name">имя маршрута</param>
        /// <param name="engine">ядро системы</param>
        /// <param name="innerRoute">маршрут, на базе которого будет работать контентная маршрутизация</param>
        /// <returns>зарегистрированный маршрут</returns>
        public static ContentRoute MapContentRoute(this RouteCollection routes,
            string name, IEngine engine, Route innerRoute = null)
        {
            Throws.IfArgumentNull(engine, _ => engine);
            Throws.IfArgumentNullOrEmpty(name, _ => name);
            //var nonContentRoutes = SelectRoutesWithIndices(routes)
            //    .Where(x => !(x.Value is ContentRoute));
            //int indexOfFirstNonContentRoute = nonContentRoutes.Any()
            //    ? nonContentRoutes.Select(i => i.Key).FirstOrDefault()
            //    : routes.Count;

            var cr = new ContentRoute(engine, null, null, innerRoute);

            //routes.Insert(indexOfFirstNonContentRoute, cr);
            routes.Add(name, cr);
            return cr;
        }
        /// <summary>
        /// Устанавливает маршрут страниц и виджетов.
        /// т.е адреса вида: company\...\about\jobs\programmer
        /// HtmlPage?page=1234&amp;part=12345
        /// </summary>
        /// <param name="name">имя маршрута</param>
        /// <param name="engine">ядро системы</param>
        /// <param name="url">шаблон адреса</param>
        /// <param name="defaults">параметры</param>
        /// <returns>зарегистрированный маршрут</returns>
        public static ContentRoute MapContentRoute(this RouteCollection routes, string name,
            IEngine engine, string url, object defaults)
        {
            Throws.IfArgumentNull(engine, _ => engine);
            Throws.IfArgumentNullOrEmpty(name, _ => name);
            Throws.IfArgumentNullOrEmpty(url, _ => url);

            //var nonContentRoutes = SelectRoutesWithIndices(routes)
            //    .Where(x => !(x.Value is ContentRoute));
            //int indexOfFirstNonContentRoute = nonContentRoutes.Any()
            //    ? nonContentRoutes.Select(i => i.Key).FirstOrDefault()
            //    : routes.Count;

            var cr = new ContentRoute(engine, null, null, new LowercaseRoute(url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(),
                new RouteValueDictionary(new { engine = engine }),
                new MvcRouteHandler()));

            routes.Add(name, cr);
            //routes.Insert(indexOfFirstNonContentRoute, cr);
            return cr;
        }

        public static ContentRoute MapContentRoute<T>(this RouteCollection routes, string name, IEngine engine, Route innerRoute = null) where T : AbstractItem
        {
            Throws.IfArgumentNull(engine, _ => engine);
            Throws.IfArgumentNullOrEmpty(name, _ => name);
            //var nonContentRoutesNorGenericRoutes = SelectRoutesWithIndices(routes)
            //    .Where(x => !(x.Value is ContentRoute)
            //        || !x.Value.GetType().ContainsGenericParameters);

            //int indexOfFirstNonContentRoute = nonContentRoutesNorGenericRoutes
            //    .Any()
            //    ? nonContentRoutesNorGenericRoutes
            //        .Select(i => i.Key)
            //        .FirstOrDefault()
            //    : routes.Count;

            var cr = new ContentRoute<T>(engine, null, null, innerRoute);

            var controllerMapper = engine.Resolve<IControllerMapper>();

            PathDictionary.PrependFinder(typeof(T), new CustomRouteTypeFinder(controllerMapper,
                controllerMapper.GetControllerName(typeof(T)),
                innerRoute));

            //routes.Insert(indexOfFirstNonContentRoute, cr);
            routes.Add(name, cr);
            return cr;
        }

        public static ContentRoute MapContentRoute<T>(this AreaRegistrationContext arc)
            where T : AbstractItem
        {
            var state = arc.State as AreaRegistrationState;
            Throws.IfArgumentNull(state, x => state);
            Throws.IfArgumentNull(state.Engine, x => state.Engine);

            var routeHandler = new MvcRouteHandler();
            var controllerMapper = state.Engine.Resolve<IControllerMapper>();
            var innerRoute = new LowercaseRoute("{area}/{controller}/{action}",
                new RouteValueDictionary(new { action = "Index" }),
                new RouteValueDictionary(),
                new RouteValueDictionary(new { engine = state.Engine, area = arc.AreaName }),
                routeHandler);

            var cr = new ContentRoute<T>(state.Engine, routeHandler, controllerMapper, innerRoute);
            arc.Routes.Add(arc.AreaName + "_" + typeof(T).FullName, cr);
            return cr;
        }

        private static IEnumerable<KeyValuePair<int, RouteBase>> SelectRoutesWithIndices(RouteCollection routes)
        {
            return routes.Select((rb, i) => new KeyValuePair<int, RouteBase>(i, rb));
        }
    }

    public class AreaRegistrationState
    {
        public AreaRegistrationState(IEngine engine)
        {
            Engine = engine;
            Values = new Dictionary<string, object>();
        }

        public IEngine Engine { get; set; }
        public IDictionary<string, object> Values { get; set; }
    }
}
