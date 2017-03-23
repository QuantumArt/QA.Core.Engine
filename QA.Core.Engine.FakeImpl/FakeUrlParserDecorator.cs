using System;
using System.Diagnostics;
using QA.Core.Cache;
using QA.Core.Engine.Web;


namespace QA.Core.Engine.Data
{
    public class FakeUrlParserDecorator : IUrlParser
    {
        readonly IUrlParser _inner;
        readonly IPersister _persister;
        TimeSpan _expiration;
        private ICacheProvider _cacheProvider;
        private ICultureUrlResolver _cultureResolver;

        public FakeUrlParserDecorator(IPersister persister, ICacheProvider cacheProvider, ICultureUrlResolver cultureResolver)
        {
            this._inner = ObjectFactoryBase.Resolve<IUrlParser>("original");
            this._persister = persister;
            _cacheProvider = cacheProvider;
            _expiration = TimeSpan.FromSeconds(60);
            _cultureResolver = cultureResolver;
        }

        //public event EventHandler<PageNotFoundEventArgs> PageNotFound
        //{
        //    add { _inner.PageNotFound += value; }
        //    remove { _inner.PageNotFound -= value; }
        //}

        public AbstractItem StartPage
        {
            get { return _inner.StartPage; }
        }

        public AbstractItem CurrentPage
        {
            get { return _inner.CurrentPage; }
        }
        public TimeSpan Expiration
        {
            get { return _expiration; }
            set { _expiration = value; }
        }

        public string BuildUrl(AbstractItem item)
        {
            var result = _inner.BuildUrl(item);
            return result;
        }

        public string BuildUrl(AbstractItem item, string region, string culture)
        {
            var result = _inner.BuildUrl(item, region, culture);
            return result;
        }

        public bool IsRootOrStartPage(AbstractItem item)
        {
            return _inner.IsRootOrStartPage(item);
        }

        public PathData ResolvePath(Url url)
        {
            return ResolvePath(url, false);
        }
        public PathData ResolvePath(Url url, bool reusable)
        {
            string key = url.ToString().ToLowerInvariant();

            PathData data = _cacheProvider.Get(key) as PathData;
            if (data == null)
            {
                data = _inner.ResolvePath(url, reusable);
                if (!data.IsEmpty() && data.IsCacheable)
                {
                    Debug.WriteLine("Adding " + url + " to cache");
                    _cacheProvider.Set(key, data.Detach(), (int)_expiration.TotalSeconds);
                }
            }
            else
            {
                Debug.WriteLine("Retrieving " + url + " from cache");
                data = data.Attach(_persister);
                data.UpdateParameters(Url.Parse(url).GetQueries());
            }

            return data;
        }


        public AbstractItem Parse(string url)
        {
            return _inner.Parse(url);
        }

        public string StripDefaultDocument(string path)
        {
            return _inner.StripDefaultDocument(path);
        }


        public AbstractItem RootPage
        {
            get { return _inner.RootPage; }
        }

        public bool IsStartPage(AbstractItem current)
        {
            return _inner.IsRootOrStartPage(current);
        }

        #region IUrlParser Members


        public Url AddTokenToUrl(string url, string cultureToken, string regionToken)
        {
            return _inner.AddTokenToUrl(url, cultureToken, regionToken);
        }

        #endregion
    }
}