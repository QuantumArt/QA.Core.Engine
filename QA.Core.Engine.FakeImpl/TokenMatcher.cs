using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Engine.FakeImpl
{
    public class UrlTokenMatcher
    {
        private CultureUrlParserConfig _config;
        public UrlTokenMatcher(CultureUrlParserConfig config)
        {
            _config = config;

            foreach (var pattern in _config.MatchingPatterns)
            {
                Url pUrl = "http://" + pattern.Value;
                var pSegments = pUrl.GetSegments();

                var tokenName = "{culture}";

                int tokenPosition = -1;
                bool isInAuthority = false;

                ParseToken(pUrl, pSegments, tokenName, ref tokenPosition, ref isInAuthority);

                pattern.ProcessCultureTokens = tokenPosition != -1;
                pattern.IsCultureInAuthority = isInAuthority;
                pattern.CultureTokenPosition = tokenPosition;

                tokenName = "{region}";

                tokenPosition = -1;
                isInAuthority = false;

                ParseToken(pUrl, pSegments, tokenName, ref tokenPosition, ref isInAuthority);

                pattern.ProcessRegionTokens = tokenPosition != -1;
                pattern.IsRegionInAuthority = isInAuthority;
                pattern.RegionTokenPosition = tokenPosition;
            }
        }

        public UrlMatchingResult Match(Url originalUrl,
            IEnumerable<string> regionTokens,
            IEnumerable<string> cultureTokens,
            bool sanitize = true)
        {
            Url pUrl = originalUrl;

            var pSegments = pUrl.GetSegments();

            string[] domains = null;

            foreach (var pattern in _config.MatchingPatterns)
            {
                UrlMatchingResult regionResult = new UrlMatchingResult();
                Url sanitized = pUrl;

                if (domains == null && pattern.IsRegionInAuthority || pattern.IsCultureInAuthority)
                {
                    domains = pUrl.Authority
                            .SplitString('.')
                            .Reverse()
                            .ToArray();
                }

                if (pattern.ProcessRegionTokens)
                {
                    string r = null;
                    if (pattern.IsRegionInAuthority)
                    {
                        var success = MatchToken(domains, pattern.RegionTokenPosition, regionTokens, out r);
                        if (!success)
                            continue;
                    }
                    else
                    {
                        MatchToken(pSegments, pattern.RegionTokenPosition, regionTokens, out r);
                        if (r != null)
                        {
                            sanitized = sanitized.RemoveSegment(pattern.RegionTokenPosition);
                        }
                    }
                    if (!string.IsNullOrEmpty(r))
                    {
                        regionResult.Pattern = pattern;
                        regionResult.Region = r;
                    }
                }

                if (pattern.ProcessCultureTokens)
                {
                    string r = null;
                    if (pattern.IsCultureInAuthority)
                    {
                        var success = MatchToken(domains, pattern.CultureTokenPosition, cultureTokens, out r);
                        if (!success)
                            continue;
                    }
                    else
                    {
                        MatchToken(pSegments, pattern.CultureTokenPosition, cultureTokens, out r);

                        if (r != null)
                        {
                            sanitized = sanitized.RemoveSegment(pattern.CultureTokenPosition);
                        }
                        else if (pattern.IsRegionInAuthority ||
                            pattern.CultureTokenPosition > pattern.RegionTokenPosition)
                        {
                            r = pattern.DefaultCultureToken;
                        }
                    }
                    if (!string.IsNullOrEmpty(r))
                    {
                        regionResult.Pattern = pattern;
                        regionResult.Culture = r;
                    }
                }
                else if (!string.IsNullOrEmpty(pattern.DefaultCultureToken))
                {
                    regionResult.Pattern = pattern;
                    regionResult.Culture = pattern.DefaultCultureToken;
                }

                regionResult.IsMatch = !(pattern.ProcessCultureTokens && string.IsNullOrEmpty(regionResult.Culture));

                if (regionResult.IsMatch)
                {
                    regionResult.SanitizedUrl = sanitized;
                    return regionResult;
                }
            }

            return UrlMatchingResult.Empty;
        }

        public Url ReplaceTokens(Url original,
            IEnumerable<string> regionTokens,
            IEnumerable<string> cultureTokens,
            string culture,
            string region,
            bool sanitize)
        {
            Url url = original;

            if (_config.MatchingPatterns.Count == 0)
            {
                return url;
            }

            if (sanitize)
            {
                var result = Match(original, regionTokens, cultureTokens, true);
                if (result.IsMatch)
                {
                    url = result.SanitizedUrl;
                }
                else
                {
                    // не удалось определить наложить шаблон на адрес. 
                    // значит, нельзя добавлять токены, тк адрес может не быть регионально-зависимым
                    // например, stage.bee.ru при шаблоне {region}.bee.ru/{culture}
                    return url;
                }
            }

            // todo сделать выбор по критерию
            var pattern = _config
                .MatchingPatterns
                .FirstOrDefault(x => x.UseForReplacing);

            if (pattern == null)
            {
                pattern = _config
                   .MatchingPatterns
                   .FirstOrDefault();
            }

            url = ReplaceByPattern(url, pattern, culture, region);

            return url;
        }

        private Url ReplaceByPattern(Url original,
            UrlMatchingPattern pattern,
            string culture,
            string region)
        {
            Url url = original;
            List<string> domains = null;
            List<string> segments = original.GetSegments().ToList();
            if (culture == null)
                culture = pattern.DefaultCultureToken ?? "";

            bool isCultureDefault = culture.Equals(pattern.DefaultCultureToken, StringComparison.OrdinalIgnoreCase);
            int culturePosition = -1;

            if (!string.IsNullOrEmpty(culture))
            {
                if (pattern.ProcessCultureTokens)
                {
                    culturePosition = SetToken(original, pattern.CultureTokenPosition, pattern.IsCultureInAuthority, culture,
                        ref url, ref domains, segments);
                }
            }

            if (!string.IsNullOrEmpty(region))
            {
                if (pattern.ProcessRegionTokens)
                {
                    SetToken(original, pattern.RegionTokenPosition, pattern.IsRegionInAuthority, region,
                        ref url, ref domains, segments);
                }
            }

            if (domains != null)
            {
                url = url.SetAuthority(string.Join(".", domains.Reverse<string>()));
            }

            if (segments.Count > 0)
            {
                if (isCultureDefault && culturePosition >= 0)
                {
                    segments.RemoveAt(culturePosition);
                }

                url = url.SetPath("/" + string.Join("/", segments));
            }

            return url;
        }

        private static int SetToken(Url original, int position, bool isInAuthority, string tokenValue,
            ref Url url, ref List<string> domains, List<string> segments, bool isDefault = false)
        {
            if (isInAuthority)
            {
                if (original.IsAbsolute)
                {
                    if (domains == null)
                    {
                        domains = GetReversedDomains(original)
                            .ToList();
                    }

                    if (domains.Count > position)
                    {
                        domains[position] = tokenValue;
                    }
                    else if (domains.Count == position)
                    {
                        domains.Add(tokenValue);
                    }
                }
            }
            else
            {
                if (segments.Count >= position)
                {
                    segments.Insert(position, tokenValue);
                    return position;

                }
                else
                {
                    segments.Add(tokenValue);
                    return 0;
                }
            }
            return -1;
        }


        private bool MatchToken(string[] segments, int tokenPosition, IEnumerable<string> values, out string result)
        {
            if (segments.Length > tokenPosition)
            {
                result = MatchToken(segments[tokenPosition], values);
                return result != null;
                    
            }
            result = null;
            return true;
        }

        private string MatchToken(string sample, IEnumerable<string> items)
        {
            if (string.IsNullOrEmpty(sample))
            {
                return null;
            }

            var s = sample.ToLower();

            return items
                .FirstOrDefault(x => x.Equals(s));
        }

        private static void ParseToken(Url pUrl, string[] pSegments, string tokenName, ref int tokenPosition, ref bool isInAuthority)
        {
            if (pUrl.Authority.Contains(tokenName))
            {
                var domains = GetReversedDomains(pUrl);

                tokenPosition = Array.IndexOf(domains, tokenName);

                isInAuthority = true;
            }
            else
            {
                tokenPosition = Array.IndexOf(pSegments, tokenName);
            }
        }

        private static string[] GetReversedDomains(Url pUrl)
        {
            var domains = pUrl.Authority
                .SplitString('.')
                .Reverse()
                .ToArray();
            return domains;
        }

    }
}
