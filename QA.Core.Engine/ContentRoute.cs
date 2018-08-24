// Owners: Karlov Nikolay

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using QA.Core.Engine.Web.Mvc;
using System.Globalization;
using QA.Core.Engine.Web;
using System.Text.RegularExpressions;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Роут для контента
    /// </summary>
    public class ContentRoute : RouteBase
    {
        private const string _savedPathDataItemsKey = @"ContentRoute\<ui-item>\saved_key";
        private const string _savedRouteDataItemsKey = @"ContentRoute\<routedata>\saved_key";
        private const string _savedRouteValuesItemsKey = @"ContentRoute\<routevalues>\saved_key";
        static ContentRoute()
        {
            IgnoreVirtualPath = false;
        }

        public static string StartPageKey = "current-startpage";
        public static string RootPageKey = "current-rootpage";

        public static bool IgnoreVirtualPath
        {
            get;
            set;
        }

        /// <summary>
        /// Ключ для страницы или виджета в строке запроса
        /// </summary>
        public static string AbstractItemKey
        {
            get { return PathData.ItemQueryKey; }
        }

        /// <summary>
        /// Ключ для страницы в строке запроса
        /// </summary>
        public static string ContentPageKey
        {
            get { return PathData.PageQueryKey; }
        }

        /// <summary>
        /// Ключ для виджета в строке запроса
        /// </summary>
        public static string ContentPartKey
        {
            get { return PathData.PartQueryKey; }
        }

        /// <summary>
        /// Ключ для культуры в строке запроса
        /// </summary>
        public static string CultureQueryKey
        {
            get { return PathData.CultureQueryKey; }
        }


        /// <summary>
        /// Ключ для региона в строке запроса
        /// </summary>
        public static string RegionQueryKey
        {
            get { return PathData.RegionQueryKey; }
        }

        public const string ContentEngineKey = "engine";
        public const string ControllerKey = "controller";
        public const string AreaKey = "area";
        public const string ActionKey = "action";

        protected readonly IEngine _engine;
        protected readonly IRouteHandler _routeHandler;
        protected readonly IControllerMapper _controllerMapper;
        protected readonly Route _innerRoute;

        internal static readonly Regex TokenPattern = new Regex(@"^[a-zA-Z_\-0-9]+$");

        public ContentRoute(IEngine engine)
            : this(engine, null, null, null)
        {
        }

        public ContentRoute(IEngine engine, IRouteHandler routeHandler, IControllerMapper controllerMapper, Route innerRoute)
        {
            this._engine = engine;
            this._routeHandler = routeHandler ?? new MvcRouteHandler();
            this._controllerMapper = controllerMapper ?? engine.Resolve<IControllerMapper>();
            this._innerRoute = innerRoute ?? new LowercaseRoute("{controller}/{action}",
                new RouteValueDictionary(new { action = "Index" }),
                new RouteValueDictionary(),
                new RouteValueDictionary(new { engine = this._engine }),
                this._routeHandler);

        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public virtual RouteValueDictionary GetRouteValues(AbstractItem item, RouteValueDictionary routeValues)
        {
            string actionName = "Index";

            if (routeValues.ContainsKey(ActionKey))
            {
                actionName = (string)routeValues[ActionKey];
            }

            string controllerName = _controllerMapper.GetControllerName(item.GetContentType());
            if (controllerName == null || !_controllerMapper.ControllerHasAction(controllerName, actionName))
            {
                return null;
            }

            var values = new RouteValueDictionary(routeValues);

            foreach (var kvp in _innerRoute.Defaults)
            {
                if (!values.ContainsKey(kvp.Key))
                {
                    values[kvp.Key] = kvp.Value;
                }
            }

            values[ControllerKey] = controllerName;
            values[ActionKey] = actionName;
            values[AbstractItemKey] = item.Id;
            values[AreaKey] = _innerRoute.DataTokens["area"];

            return values;
        }

        /// <summary>
        /// Получение параметров маршрутизации для запроса
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var data = GetSavedRouteData(httpContext) ??
                    GetRouteDataInternal(httpContext);

            return data;
        }

        private RouteData GetRouteDataInternal(HttpContextBase httpContext)
        {
            string path = ExtractPath(httpContext.Request);

            if (path.EndsWith(".axd", StringComparison.InvariantCultureIgnoreCase))
                return new RouteData(this, new StopRoutingHandler());
            if (path.EndsWith(".ashx", StringComparison.InvariantCultureIgnoreCase))
                return new RouteData(this, new StopRoutingHandler());

            RouteData routeData = null;

            if (httpContext.Request.QueryString[ContentPartKey] != null)
            {
                routeData = CheckForContentController(httpContext);
            }

            if (routeData == null)
            {
                routeData = GetRouteDataForPath(httpContext.Request);
            }

            if (routeData == null)
            {
                routeData = CheckForContentController(httpContext);
            }

            Debug.WriteLine("GetRouteData for '" + path + "' got values: " + (routeData != null ? routeData.Values.ToQueryString() : "(null)"));

            return routeData;
        }

        /// <summary>
        /// Получение всех данных по Url
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private RouteData GetRouteDataForPath(HttpRequestBase request)
        {
            string host = (request.Url.IsDefaultPort) ? request.Url.Host : request.Url.Authority;
            string hostAndRawUrl = String.Format("{0}://{1}{2}", request.Url.Scheme, host,
                (IgnoreVirtualPath ? request.Path : Url.ToAbsolute(ExtractPath(request))));

            PathData td = (request.RequestContext.HttpContext.Items[_savedPathDataItemsKey] as PathData) ??
                _engine.UrlParser.ResolvePath(hostAndRawUrl);

            if (!td.IsEmpty())
            {
                request.RequestContext.HttpContext.Items[_savedPathDataItemsKey] = td;
            }

            var page = td.CurrentPage;

            var actionName = td.Action;
            if (string.IsNullOrEmpty(actionName))
                actionName = request.QueryString["action"] ?? "Index";

            if (!string.IsNullOrEmpty(request.QueryString[PathData.PageQueryKey]))
            {
                int pageId;
                if (int.TryParse(request.QueryString[PathData.PageQueryKey], out pageId))
                {
                    // взять из базы или из кеша
                    td.CurrentPage = page = _engine.Persister.Get(pageId);
                }
            }

            AbstractItem part = null;
            if (!string.IsNullOrEmpty(request.QueryString[PathData.PartQueryKey]))
            {
                int partId;
                if (int.TryParse(request.QueryString[PathData.PartQueryKey], out partId))
                {
                    // взять из базы или из кеша
                    td.CurrentItem = part = _engine.Persister.Get(partId);
                }
            }

            if (page == null && part == null)
                return null;

            else if (page == null)
            {
                page = part.ClosestPage();
            }

            var controllerName = _controllerMapper.GetControllerName((part ?? page).GetContentType());

            if (controllerName == null)
                return null;

            if (actionName == null || !_controllerMapper.ControllerHasAction(controllerName, actionName))
                return null;

            var data = SetRouteValues(page, td);

            if (data == null)
            {
                return null;
            }

            if (data.Values.ContainsKey("action"))
                actionName = data.Values["action"].ToString();

            foreach (var tokenPair in _innerRoute.DataTokens)
            {
                data.DataTokens[tokenPair.Key] = tokenPair.Value;
            }

            data.DataTokens["CurrentCultureToken"] = td.CultureToken;
            data.DataTokens["CurrentRegionToken"] = td.RegionToken;

            // workaround for routes with id token applied to NewsPage for QP8 data
            if (td.RouteData == null && _innerRoute.Url == ("{controller}/{action}/{id}") && !string.IsNullOrEmpty(td.Argument))
            {
                data.Values["id"] = td.Argument.TrimEnd('/');
            }

            HttpContext.Current.Items["TrailedUrl"] = td.TrailedUrl;
            //}

            // data.Values["argument"] = td.Argument;
            RouteExtensions.ApplyCurrentItem(data, controllerName, actionName, page, part);
            data.DataTokens[ContentEngineKey] = _engine;

            return data;
        }

        protected virtual RouteData SetRouteValues(AbstractItem item, PathData td)
        {
            var data = new RouteData(this, _routeHandler);

            foreach (var defaultPair in td.RouteData == null ? _innerRoute.Defaults : td.RouteData.Values)
            {
                if (defaultPair.Key != "action" && defaultPair.Key != "controller")
                    data.Values[defaultPair.Key] = defaultPair.Value;
            }

            return data;
        }

        private static string ExtractPath(HttpRequestBase request)
        {
            return request.AppRelativeCurrentExecutionFilePath;
        }

        /// <summary>Обрабатывает запрос вида /{controller}/{action}/?ui-page=123&amp;ui-item=234</summary>
        private RouteData CheckForContentController(HttpContextBase context)
        {
            var routeData = _innerRoute.GetRouteData(context);

            if (routeData == null)
                return null;

            var controllerName = Convert.ToString(routeData.Values[ControllerKey]);
            var actionName = Convert.ToString(routeData.Values[ActionKey]);

            if (!_controllerMapper.ControllerHasAction(controllerName, actionName))
            {
                return null;
            }

            // установка языка из параметров запроса
            var cultureToken = context.Request.QueryString[CultureQueryKey];
            if (!string.IsNullOrEmpty(cultureToken))
            {
                if (ValidateCultureToken(cultureToken))
                {
                    routeData.Values[CultureQueryKey] = cultureToken;
                }
            }

            // установка региона из параметор запроса
            var regionToken = context.Request.QueryString[RegionQueryKey];
            if (!string.IsNullOrEmpty(regionToken))
            {
                if (ValidateRegionToken(regionToken))
                {
                    routeData.Values[RegionQueryKey] = regionToken;
                }
            }
            AbstractItem item = null;
            var part = ApplyContent(routeData, context.Request.QueryString, ContentPartKey);
            if (part != null)
            {
                routeData.ApplyAbstractItem(AbstractItemKey, part);
            }
            else
            {
                item = ApplyContent(routeData, context.Request.QueryString, AbstractItemKey);
            }

            var page = ApplyContent(routeData, context.Request.QueryString, ContentPageKey);
            if (page == null)
            {
                if (part == null && item == null)
                {
                    throw new HttpException(404, "page is not found");
                }
                else if (part != null)
                {
                    routeData.ApplyAbstractItem(ContentPageKey, part.ClosestPage());
                }
                else
                {
                    routeData.ApplyAbstractItem(ContentPageKey, item.ClosestPage());
                }
            }
            routeData.DataTokens[ContentEngineKey] = _engine;

            return routeData;
        }

        private bool ValidateRegionToken(string regionToken)
        {
            return TokenPattern.IsMatch(regionToken);
        }

        private bool ValidateCultureToken(string cultureToken)
        {
            return TokenPattern.IsMatch(cultureToken);
        }

        private AbstractItem ApplyContent(RouteData routeData, NameValueCollection query, string key)
        {
            int id;
            if (int.TryParse(query[key], out id))
            {
                var item = _engine.Persister.Get(id);
                routeData.ApplyAbstractItem(key, item);
                return item;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            AbstractItem item;

            values = new RouteValueDictionary(values);

            if (!TryConvertContentToController(requestContext, values, AbstractItemKey, out item))
            {
                item = requestContext.CurrentItem();

                if (item == null)
                {
                    return null;
                }

                if (!RequestedControllerMatchesItemController(values, item))
                {
                    return null;
                }
            }

            if (item.IsPage)
            {
                return ResolveContentActionUrl(requestContext, values, item);
            }

            AbstractItem page = values.CurrentItem<AbstractItem>(ContentPageKey, _engine.Persister);

            if (page != null)
            {
                return ResolvePartActionUrl(requestContext, values, page, item);
            }

            page = requestContext.CurrentPage<AbstractItem>();

            if (page != null && page.IsPage)
            {
                return ResolvePartActionUrl(requestContext, values, page, item);
            }

            page = item.ClosestPage();

            if (page != null && page.IsPage)
            {
                return ResolvePartActionUrl(requestContext, values, page, item);
            }

            return null;
        }

        private bool RequestedControllerMatchesItemController(RouteValueDictionary values, AbstractItem item)
        {
            string requestedController = values[ControllerKey] as string;
            if (requestedController == null)
            {
                return true;
            }

            string itemController = _controllerMapper.GetControllerName(item.GetContentType());

            return string.Equals(requestedController, itemController, StringComparison.InvariantCultureIgnoreCase);
        }

        private VirtualPathData ResolvePartActionUrl(RequestContext requestContext, RouteValueDictionary values, AbstractItem page, AbstractItem item)
        {
            values[ContentPageKey] = page.Id;
            values[ContentPartKey] = item.Id;
            values[CultureQueryKey] = CultureInfo.CurrentUICulture.Name;

            if (!string.IsNullOrEmpty(requestContext.HttpContext.Request.QueryString["editing"]))
            {
                values["editing"] = true;
            }

            var resolver = _engine.Resolve<ICultureUrlResolver>();

            if (resolver.IsResolved)
            {
                values[RegionQueryKey] = resolver.GetCurrentRegion();
                values[CultureQueryKey] = resolver.GetCurrentCulture();
            }
            else
            {
                values[CultureQueryKey] = CultureInfo.CurrentUICulture.Name;
            }

            var vpd = _innerRoute.GetVirtualPath(requestContext, values);

            if (vpd != null)
            {
                vpd.Route = this;
            }

            return vpd;
        }

        private VirtualPathData ResolveContentActionUrl(RequestContext requestContext, RouteValueDictionary values, AbstractItem item)
        {
            const string controllerPlaceHolder = "---(ctrl)---";
            const string areaPlaceHolder = "---(area)---";

            var route = _innerRoute;

            values[ControllerKey] = controllerPlaceHolder;
            bool useAreas = route.DataTokens.ContainsKey("area");
            if (useAreas)
            {
                values[AreaKey] = areaPlaceHolder;
            }

            if (!string.IsNullOrEmpty(requestContext.HttpContext.Request.QueryString["editing"]))
            {
                values["editing"] = true;
            }

            if (values.ContainsKey(AbstractItemKey))
            {
                values.Remove(AbstractItemKey);
            }

            VirtualPathData vpd = route.GetVirtualPath(requestContext, values);

            if (vpd == null)
            {
                return null;
            }

            vpd.Route = this;

            string relativeUrl = Url.ToRelative(item.Url, IgnoreVirtualPath);
            Url actionUrl = vpd.VirtualPath.Replace(controllerPlaceHolder, Url.PathPart(relativeUrl).TrimStart('~'));

            if (useAreas)
            {
                actionUrl = actionUrl.SetPath(actionUrl.Path.Replace(areaPlaceHolder + "/", ""));
            }

            foreach (var kvp in Url.ParseQueryString(Url.QueryPart(relativeUrl)))
            {
                // item
                if (AbstractItemKey.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                actionUrl = actionUrl.AppendQuery(kvp.Key, kvp.Value);
            }

            if (!string.IsNullOrEmpty(actionUrl.Path) && !actionUrl.Path.EndsWith("/"))
            {
                actionUrl = actionUrl.SetPath(actionUrl.Path + "/");
            }

            vpd.VirtualPath = actionUrl.PathAndQuery.TrimStart('/').ToLower();

            return vpd;
        }

        private bool TryConvertContentToController(RequestContext request, RouteValueDictionary values, string key, out AbstractItem item)
        {
            if (!values.ContainsKey(key))
            {
                item = null;
                return false;
            }

            object value = values[key];
            item = value as AbstractItem;
            if (item == null && value is int)
            {
                // взять из БД
                item = _engine.Persister.Get((int)value);
            }
            else if (value is string)
            {
                int id = 0;
                if (int.TryParse((string)value, out id))
                {
                    item = _engine.Persister.Get(id);
                }
            }

            if (item == null || item == request.CurrentItem())
            {

                return false;
            }

            values.Remove(key);
            values[ControllerKey] = _controllerMapper.GetControllerName(item.GetContentType());

            return true;
        }

        protected void SaveRouteData(RouteData routeData, HttpContextBase context)
        {
            context.Items[_savedRouteDataItemsKey] = routeData;
        }

        protected RouteData GetSavedRouteData(HttpContextBase context)
        {
            RouteData routeData = null;
            if (context.Items.Contains(_savedRouteDataItemsKey))
            {
                routeData = (RouteData)context.Items[_savedRouteDataItemsKey];
                context.Items.Remove(_savedRouteDataItemsKey);
            }
            return routeData;
        }

    }


}
