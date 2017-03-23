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
        protected static string[] _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(x => x.Name.ToLower()).ToArray();

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
            var matcher = GetMatcher();

            var result = matcher.Match(url, GetRegionCodes(), GetValidCultures(), true);

            _isResolved.Value = true;
            _currentResult.Value = result;

            if (result.IsMatch)
            {
                regionToken = result.Region;

                var queryToken = url.GetQuery(PathData.CultureQueryKey);

                if (!string.IsNullOrWhiteSpace(queryToken))
                {
                    if (ContentRoute.TokenPattern.IsMatch(queryToken))
                    {
                        result.Culture = queryToken.Trim();
                    }
                }

                cultureToken = result.Culture;
                return result.SanitizedUrl;
            }


            regionToken = null;
            cultureToken = null;

            return url;
        }


        public virtual Url ResolveCultureReusable(Url url, out string cultureToken, out string regionToken, bool replaceUrl)
        {
            var matcher = GetMatcher();

            var result = matcher.Match(url, GetRegionCodes(), GetValidCultures(), true);

            if (result.IsMatch)
            {
                regionToken = result.Region;

                var queryToken = url.GetQuery(PathData.CultureQueryKey);

                if (!string.IsNullOrWhiteSpace(queryToken))
                {
                    if (ContentRoute.TokenPattern.IsMatch(queryToken))
                    {
                        result.Culture = queryToken.Trim();
                    }
                }

                cultureToken = result.Culture;
                return result.SanitizedUrl;
            }


            regionToken = null;
            cultureToken = null;

            return url;
        }

        public virtual Url AddTokensToUrl(Url originalUrl, string cultureToken, string regionToken)
        {
            return AddTokensToUrl(originalUrl, cultureToken, regionToken, false);
        }

        public virtual Url AddTokensToUrl(Url originalUrl, string cultureToken, string regionToken, bool sanitize)
        {
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
