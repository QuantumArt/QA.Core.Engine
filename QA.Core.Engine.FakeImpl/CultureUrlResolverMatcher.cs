using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Data;
#pragma warning disable 1591

namespace QA.Core.Engine.FakeImpl
{
    /// <summary>
    /// Реализация CultureUrlResolver, позволяющая указывать регионы и шаблоны адресов
    /// </summary>
    public class CultureUrlResolverMatcher : CultureUrlResolver
    {
        private List<UrlMatchingPattern> _patterns;
        private IEnumerable<Region> _regions;

        public IReadOnlyList<UrlMatchingPattern> Patterns
        {
            get { return _patterns; }
        }

        public CultureUrlResolverMatcher(IEnumerable<UrlMatchingPattern> matchers)
        {
            _patterns = new List<UrlMatchingPattern>(matchers);
        }

        public CultureUrlResolverMatcher(IEnumerable<UrlMatchingPattern> matchers, IEnumerable<Region> regions)
        {
            _patterns = new List<UrlMatchingPattern>(matchers);
            _regions = regions;
        }

        protected override IEnumerable<string> GetRegionCodes()
        {
            if(_regions != null)
            {
                return _regions.Select(x => x.Alias);
            }

            return base.GetRegionCodes();
        }

        protected override UrlTokenMatcher GetMatcher()
        {
            return new UrlTokenMatcher(new CultureUrlParserConfig() { MatchingPatterns = _patterns });
        }
    }
}
