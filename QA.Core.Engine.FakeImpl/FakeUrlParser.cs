using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using QA.Core.Engine.Web;
using QA.Core.Web;

namespace QA.Core.Engine.Data
{
    public class FakeUrlParser : IUrlParser
    {
        public event EventHandler<PageNotFoundEventArgs> PageNotFound;
        private static RequestLocal<AbstractItem> _startPage = new RequestLocal<AbstractItem>();
        private static RequestLocal<AbstractItem> _rootPage = new RequestLocal<AbstractItem>();
        private static RequestLocal<AbstractItem> _currentPage = new RequestLocal<AbstractItem>();
        private IEngine _engine;

        private IPersister _persister;
        private ICultureUrlResolver _cultureResolver;
        private IStartPageProvider _startPageProvider;

        public AbstractItem StartPage
        {
            get
            {
                var item = _startPage.Value;

                if (item != null)
                {
                    return item;
                }
                else
                {
                    var page = _persister.Get(StartPageId);
                    _startPage.Value = page;
                    return page;
                }
            }
        }

        public AbstractItem RootPage
        {
            get
            {
                return _startPageProvider.GetRootPage();
            }
        }

        public AbstractItem CurrentPage
        {
            get
            {
                return _currentPage.Value;
            }
        }

        protected int StartPageId
        {
            get { return _startPageProvider.GetStartPageId(); }
        }

        protected int RootPageId
        {
            get { return _startPageProvider.GetRootPageId(); }
        }

        public FakeUrlParser(IEngine engine)
        {
            _cultureResolver = engine.Resolve<ICultureUrlResolver>();
            _engine = engine;
            _startPageProvider = engine.Resolve<IStartPageProvider>();
            _persister = engine.Persister;
        }

        public virtual string BuildUrl(AbstractItem item, string regionToken, string cultureToken)
        {
            if (item.Culture != null)
            {
                cultureToken = item.Culture.Key;
            }

            bool isStartPage = false;
            if (item == null) throw new ArgumentNullException("item");

            AbstractItem current = item;

            if (item.VersionOf != null)
            {
                current = item.VersionOf;
            }

            // двигаем вверх до 1-й страницы
            while (current != null && !current.IsPage)
            {
                current = current.Parent;
            }

            // no page found
            if (current == null) throw new Exception("Cannot build url to data item '{0}' with no containing page item.");

            Url url;
            if (IsStartPage(current))
            {
                isStartPage = true;
                url = "/";
                // TODO: set authority. Check out requirements
                //url = url.SetAuthority(_startPageProvider.GetBinding(current.Id));
            }
            else
            {
                //TODO: обрабатывать контентные версии
                current = current.GetClosestStructural();
                url = new Url("/" + current.Name);
                current = current.Parent;

                while (current != null && !IsStartPage(current))
                {
                    url = url.PrependSegment(current.Name);
                    current = current.Parent;
                }
            }

            // исп. RewrittenUrl
            if (current == null) return item.FindPath(PathData.DefaultAction,
                regionToken, cultureToken)
                .RewrittenUrl;

            if (item.IsPage && item.VersionOf != null)
            {
                // add trailing slash

                // проверить, что токены добавлены
                // url = url.AppendQuery(PathData.PageQueryKey, item.Id);
            }
            else if (!item.IsPage)
            {
                url = url.AppendQuery(PathData.ItemQueryKey, item.Id);
            }

            //if (_cultureResolver != null)
            //{
            //    url = _cultureResolver.AddTokensToUrl(url,
            //        cultureToken, regionToken);
            //}

            if (item.IsPage && url.Path != string.Empty && !url.Path.EndsWith("/"))
            {
                url = url.SetPath(url.Path + "/");
            }

            if (!isStartPage)
            {
                return ContentRoute.IgnoreVirtualPath ? (string)url : Url.ToAbsolute("~" + url);
            }
            else
            {
                // TODO
                return ContentRoute.IgnoreVirtualPath ? (string)url : Url.ToAbsolute(url);
            }
        }

        public Url AddTokenToUrl(string url, string cultureToken, string regionToken)
        {
            return _cultureResolver.AddTokensToUrl(url, cultureToken, regionToken);
        }

        public virtual string BuildUrl(AbstractItem item)
        {
            return BuildUrl(item, _cultureResolver.GetCurrentRegion(), _cultureResolver.GetCurrentCulture());
        }

        public bool IsStartPage(AbstractItem current)
        {
            return current.Id == StartPageId || _startPageProvider.IsStartPage(current.Id);
        }

        public bool IsRootOrStartPage(AbstractItem item)
        {
            return item != null && (IsStartPage(item) || item.Id == RootPageId);
        }

        public PathData ResolvePath(Url url)
        {
            return ResolvePath(url, false);
        }

        public PathData ResolvePath(Url url, bool reusable)
        {
            if (url == null) return PathData.Empty;

            string currentCultureToken = string.Empty;
            string currentRegionToken = string.Empty;

            Url requestedUrl = url;

            // todo: resolve region and culture
            if (_cultureResolver != null)
            {
                if (reusable)
                {
                    requestedUrl = _cultureResolver.ResolveCultureReusable(requestedUrl, out currentCultureToken, out currentRegionToken, true);
                }
                else
                {
                    requestedUrl = _cultureResolver.ResolveCulture(requestedUrl, out currentCultureToken, out currentRegionToken, true);
                }
            }

            AbstractItem item = TryLoadingFromQueryString(requestedUrl, PathData.ItemQueryKey);
            AbstractItem page = TryLoadingFromQueryString(requestedUrl, PathData.PageQueryKey);

            if (page != null)
            {
                var directPath = page
                    .FindPath(requestedUrl["action"] ?? PathData.DefaultAction,
                        currentRegionToken, currentCultureToken)
                        .SetArguments(requestedUrl["arguments"])
                        .UpdateParameters(requestedUrl.GetQueries());

                var directData = UseItemIfAvailable(item, directPath);

                directData.IsRewritable &= !string.Equals(url.ApplicationRelativePath, directData.TemplateUrl, StringComparison.InvariantCultureIgnoreCase);
                return directData;
            }

            AbstractItem startPage = GetStartPage(requestedUrl);
            if (startPage == null)
                return PathData.Empty;

            string path = ContentRoute.IgnoreVirtualPath ?
                requestedUrl.Path :
                    Url.ToRelative(requestedUrl.Path).TrimStart('~');

            PathData data = startPage
                .FindPath(path, currentRegionToken, currentCultureToken)
                .UpdateParameters(requestedUrl.GetQueries());

            if (data.IsEmpty())
            {
                if (path.EndsWith(DefaultDocument, StringComparison.OrdinalIgnoreCase))
                {
                    // Try to find path without default document.
                    // TODO: inject here
                    data = StartPage
                        .FindPath(StripDefaultDocument(path), currentRegionToken, currentCultureToken)
                        .UpdateParameters(requestedUrl.GetQueries());
                }

                if (data.IsEmpty())
                {
                    // Позволяем пользовательскому коду обрабатывать page not found
                    if (PageNotFound != null)
                    {
                        PageNotFoundEventArgs args = new PageNotFoundEventArgs(requestedUrl);
                        args.AffectedPath = data;
                        PageNotFound(this, args);
                        data = args.AffectedPath;
                    }
                }
            }

            if (item != null)
            {
                if (item.IsPage)
                {
                    // это может приводить к ошибкам, если вызывается не из роутинга
                    _currentPage.Value = item;
                }
                //item.RegionTokens = currentRegionToken;
                //item.CultureToken = currentCultureToken;
            }
            if (data != null)
            {
                data.RegionToken = currentRegionToken;
                data.CultureToken = currentCultureToken;
            }
            //            data.Ignore = !IgnoreExisting(webContext.HttpContext.Request.PhysicalPath);

            data.TrailedUrl = requestedUrl;
            return UseItemIfAvailable(item, data);
        }

        public string DefaultDocument
        {
            get { return Url.DefaultDocument; }
            set { Url.DefaultDocument = value; }
        }

        public virtual AbstractItem Parse(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var requestedUrl = new Url(url);

            string token1 = null;
            string token2 = null;

            if (_cultureResolver != null)
            {
                requestedUrl = _cultureResolver.ResolveCulture(requestedUrl, out token1, out token2, true);
            }

            AbstractItem startingPoint = GetStartPage(requestedUrl);
            var item = TryLoadingFromQueryString(requestedUrl, PathData.ItemQueryKey, PathData.PageQueryKey) ?? Parse(startingPoint, requestedUrl);

            if (item != null)
            {
                if (item.IsPage)
                {
                    // это может приводить к ошибкам, если вызывается но из роутинга
                    _currentPage.Value = item;
                }
                //item.CultureToken = token1;
                //item.RegionTokens = token2;
            }

            return item;
        }

        public string StripDefaultDocument(string path)
        {
            return path;
        }

        #region Parse Helper Methods
        protected virtual AbstractItem TryLoadingFromQueryString(string url, params string[] parameters)
        {
            int? itemID = FindQueryStringReference(url, parameters);
            if (itemID.HasValue)
                return _engine.Persister.Get(itemID.Value);
            return null;
        }

        protected virtual AbstractItem Parse(AbstractItem current, string url)
        {
            if (current == null) throw new ArgumentNullException("current");

            Debug.WriteLine("Parsing " + url);
            url = CleanUrl(url);

            if (url.Length == 0)
                return current;

            return current.GetChild(url) ?? NotFoundPage(url);
        }

        /// <summary>Возвращает стартовую страницу, если адрес Default.aspx - для поддержки classicmode</summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected virtual AbstractItem NotFoundPage(string url)
        {
            if (IsDefaultDocument(url))
            {
                return StartPage;
            }

            Debug.WriteLine("No content at: " + url);

            PageNotFoundEventArgs args = new PageNotFoundEventArgs(url);
            if (PageNotFound != null)
                PageNotFound(this, args);
            return args.AffectedItem;
        }

        private string CleanUrl(string url)
        {
            url = Url.PathPart(url);
            if (!ContentRoute.IgnoreVirtualPath)
                url = Url.ToRelative(url);
            return url.TrimStart('~', '/');
        }

        private int? FindQueryStringReference(string url, params string[] parameters)
        {
            string queryString = Url.QueryPart(url);
            if (!string.IsNullOrEmpty(queryString))
            {
                string[] queries = queryString.Split('&');

                foreach (string parameter in parameters)
                {
                    int parameterLength = parameter.Length + 1;
                    foreach (string query in queries)
                    {
                        if (query.StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int id;
                            if (int.TryParse(query.Substring(parameterLength), out id))
                            {
                                return id;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static PathData UseItemIfAvailable(AbstractItem item, PathData data)
        {
            if (item != null)
            {
                data.CurrentPage = data.CurrentItem;
                data.CurrentItem = item;
            }
            return data;
        }

        protected virtual AbstractItem GetStartPage(Url url)
        {
            var id = _startPageProvider.GetStartPageId(url);

            return _persister.Get(id);
        }

        //bool IgnoreExisting(string physicalPath)
        //{
        //    return ignoreExistingFiles || (!File.Exists(physicalPath) && !Directory.Exists(physicalPath));
        //}

        bool IsDefaultDocument(string path)
        {
            return path.Equals(DefaultDocument, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion
    }
}
