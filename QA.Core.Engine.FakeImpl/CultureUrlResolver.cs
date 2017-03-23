using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QA.Core.Engine.FakeImpl;
using QA.Core.Engine.Web;
using QA.Core.Web;

namespace QA.Core.Engine.Data
{
    public class CultureUrlResolver : ICultureUrlResolver
    {
        public virtual TimeSpan CachePeriod { get; set; }
        public virtual bool ProcessRegions { get; set; }

        static string DefaultCultureToken = "ru-ru";
        static RequestLocal<UrlMatchingResult> _currentResult = new RequestLocal<UrlMatchingResult>();
        static RequestLocal<bool> _isResolved = new RequestLocal<bool>();
        protected static string[] _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => x.Name.ToLower())
            .Where(x => x.Length == 5)
            .ToArray();

        protected virtual UrlTokenMatcher GetMatcher()
        {
            var config = new CultureUrlParserConfig();

            config.MatchingPatterns.Add(
                new UrlMatchingPattern
                {
                    Value = "fakesite.ru/{culture}/{region}",
                    DefaultCultureToken = "ru-ru"
                });

            config.MatchingPatterns.Add(
                new UrlMatchingPattern
                {
                    Value = "fakesite.ru/{region}",
                    DefaultCultureToken = "ru-ru"
                });



            return new UrlTokenMatcher(config);
        }

        protected virtual IEnumerable<string> GetRegionCodes()
        {
            // только для теста. Данный метод необходимо перегрузить
            return new string[] { "msc", "spb", "primorie", "msk", "mo", "balashiha" };
        }

        protected virtual IEnumerable<string> GetValidCultures()
        {
            return _cultures;
        }

        protected virtual string GetDefaultCultureToken()
        {
            return DefaultCultureToken;
        }

        public virtual Url ResolveCulture(Url url, out string cultureToken, out string regionToken, bool replaceUrl)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            var matcher = GetMatcher();

            var codes = GetRegionCodes();

            var result = matcher.Match(url, codes, GetValidCultures(), true);

            regionToken = null;
            cultureToken = null;

            _isResolved.Value = true;
            _currentResult.Value = result;

            if (result.IsMatch)
            {
                regionToken = result.Region;
                cultureToken = result.Culture;

                CheckOverrides(url, ref result, ref cultureToken, ref regionToken);

                if (result.SanitizedUrl == null)
                    throw new ArgumentNullException("result.SanitizedUrl", string.Format("{0}: {1}",
                        url,
                        new { result, cultureToken, regionToken, result.IsMatch }));

                return result.SanitizedUrl;
            }
            else
            {
                CheckOverrides(url, ref result, ref cultureToken, ref regionToken);
            }

            return url;
        }

        private void CheckOverrides(Url url, ref UrlMatchingResult result, ref string cultureToken, ref string regionToken)
        {
            var cqueryToken = (url.GetQuery(PathData.CultureQueryKey) ?? "").ToLower();
            var rqueryToken = (url.GetQuery(PathData.RegionQueryKey) ?? "").ToLower();

            if (!string.IsNullOrWhiteSpace(cqueryToken) && GetValidCultures().Contains(cqueryToken))
            {
                cultureToken = cqueryToken;
                result.IsMatch = true;
                result.Culture = cqueryToken;
            }

            if (!string.IsNullOrWhiteSpace(rqueryToken) && GetRegionCodes().Contains(rqueryToken))
            {
                regionToken = rqueryToken;
                result.IsMatch = true;
                result.Region = rqueryToken;
            }
        }


        public virtual Url ResolveCultureReusable(Url url, out string cultureToken, out string regionToken, bool replaceUrl)
        {
            if (url == null)
                throw new ArgumentNullException("url", "before");

            var matcher = GetMatcher();

            var result = matcher.Match(url, GetRegionCodes(), GetValidCultures(), true);

            if (result.IsMatch)
            {
                regionToken = result.Region;
                cultureToken = result.Culture;

                CheckOverrides(url, ref result, ref cultureToken, ref regionToken);

                return result.SanitizedUrl;
            }


            regionToken = null;
            cultureToken = null;

            return url;
        }

        public virtual Url AddTokensToUrl(Url originalUrl, string cultureToken, string regionToken)
        {
            Throws.IfArgumentNullOrEmpty(originalUrl, _ => originalUrl);

            return AddTokensToUrl(originalUrl, cultureToken, regionToken, false);
        }

        public virtual Url AddTokensToUrl(Url originalUrl, string cultureToken, string regionToken, bool sanitize)
        {
            Throws.IfArgumentNullOrEmpty(originalUrl, _ => originalUrl);

            var matcher = GetMatcher();

            return matcher.ReplaceTokens(originalUrl, GetRegionCodes(), GetValidCultures(), cultureToken, regionToken, sanitize);
        }


        public string GetCurrentCulture()
        {
            // если не выбрасывать исключение, то ошибку очень легко не заметить
            if (_isResolved.Value != true)
            {
                throw new InvalidOperationException("Culture must be resolved from url in order to get current value.");
            }

            return _currentResult.Value.Culture ?? GetDefaultCultureToken();
        }

        public bool TrySetCulture(string token)
        {
            if (string.IsNullOrEmpty(token) && _currentResult.Value != null)
            {
                _currentResult.Value.Culture = token;
                return true;
            }

            return false;
        }

        public bool TrySetRegion(string token)
        {
            if (string.IsNullOrEmpty(token) && _currentResult.Value != null)
            {
                _currentResult.Value.Region = token;
                return true;
            }

            return false;
        }

        public string GetCurrentRegion()
        {
            if (_isResolved.Value != true)
            {
                throw new InvalidOperationException("Region must be resolved from url in order to get current value.");
            }

            return _currentResult.Value.Region;
        }

        #region ICultureUrlResolver Members


        public bool IsResolved
        {
            get { return _isResolved.Value; }
        }

        #endregion
    }
}
