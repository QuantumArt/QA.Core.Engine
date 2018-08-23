using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using QA.Core.Engine.FakeImpl;
using QA.Core.Engine.Web;
using QA.Core.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Data
{
    public class StartPageProvider : IStartPageProvider
    {
        static RequestLocal<int> _startPageId = new RequestLocal<int>();
        static RequestLocal<int> _rootPageId = new RequestLocal<int>();
        static RequestLocal<IRootPage> _rootPage = new RequestLocal<IRootPage>();
        static RequestLocal<int> _defaultStartPageId = new RequestLocal<int>();

        static RequestLocal<Dictionary<string, int>> _mappings =
            new RequestLocal<Dictionary<string, int>>(() => new Dictionary<string, int>());

        static ConcurrentDictionary<int, object> _startPageIds =
            new ConcurrentDictionary<int, object>();

        private IEngine _engine;
        static RequestLocal<WildcardMatcher> _matcher = new RequestLocal<WildcardMatcher>();

        public StartPageProvider(IEngine engine)
        {
            _engine = engine;
        }

        public int GetStartPageId()
        {
            string binding = HttpContext.Current.Request.Url.Authority;
            return GetStartPageId(binding);
        }

        protected int GetStartPageId(string currentDns)
        {
            var val = _startPageId.Value;

            if (string.IsNullOrEmpty(currentDns))
            {
                currentDns = HttpContext.Current.Request.Url.Authority;
            }

            if (_matcher.Value != null)
            {
                var value = _matcher.Value.MatchLongest(currentDns);
                if (value != null)
                {
                    int id;
                    if (_mappings.Value.TryGetValue(value, out id))
                    {
                        _startPageId.Value = id;
                        return id;
                    }
                }

                return ((IRootPage)GetRootPage()).DefaultStartPageId;
            }
            else
            {
                var root = GetRootPage();

                Dictionary<string, int> mapping = GetMappings(root);

                var matcher = _matcher.Value;

                var binding = matcher.MatchLongest(currentDns);

                if (binding != null)
                {
                    var id = mapping[binding];
                    _startPageId.Value = id;
                    return id;
                } return ((IRootPage)root).DefaultStartPageId;
            }


        }

        private Dictionary<string, int> GetMappings(AbstractItem root)
        {
            Throws.IfArgumentNull(root, _ => root);

            if (_matcher.Value == null)
            {
                _defaultStartPageId.Value = ((IRootPage)root).DefaultStartPageId;

                List<string> all = new List<string>();
                Dictionary<string, int> mapping = _mappings.Value;

                foreach (var child in root.GetChildren(GetFilter()).OfType<IStartPage>())
                {
                    var bindings = child.GetDNSBindings();
                    all.AddRange(bindings);
                    Array.ForEach(bindings, x => mapping[x] = child.Id);
                    _startPageIds[child.Id] = null;
                }
                object _ = null;
                foreach (var id in _startPageIds.Keys.ToList())
                {
                    if (!mapping.Values.Any(x => x == id))
                    {
                        _startPageIds.TryRemove(id, out _);
                    }
                }

                _matcher.Value = new WildcardMatcher(WildcardMatchingOption.FullMatch, all.Distinct());

                return mapping;
            }

            return _mappings.Value;
        }

        private Collections.ItemFilter GetFilter()
        {
            var resolver = _engine.Resolve<ICultureUrlResolver>();

            return new VersioningFilter(resolver.GetCurrentRegion(), resolver.GetCurrentCulture());
        }

        public AbstractItem GetRootPage()
        {
            var id = GetRootPageId();
            var rootPage = _engine.Persister.Get(id);
            if (rootPage == null)
            {
                throw new InvalidOperationException(string.Format("Root page with id {0} has not been loaded. Check database.", id));
            }
            return rootPage;
        }

        public int GetRootPageId()
        {
            return QPSettings.RootPageId;
        }


        public int GetStartPageId(Url url)
        {
            var dns = url.Authority;
            return GetStartPageId(dns);
        }


        public bool IsStartPage(int id)
        {
            if (_startPageIds.Keys.Count == 0)
            {
                GetMappings(GetRootPage());
            }
            return _startPageIds.ContainsKey(id);
        }
    }


}
