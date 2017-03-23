using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Collections;
using QA.Core.Engine.Interface;

namespace QA.Core.Engine.Filters
{
    public class UrlPartFilter : ItemFilter
    {
        private string _pageUrl;

        public UrlPartFilter(Url pageUrl)
        {
            _pageUrl = pageUrl;
        }

        public override bool Match(AbstractItem item)
        {
            return MatchUrl(_pageUrl, item);
        }

        internal virtual bool MatchUrl(Url url, IUrlFilterable item)
        {
            if (item.AllowedUrls == null && item.DeniedUrls == null)
                return true;

            if (item.DeniedUrls != null)
            {
                foreach (var pattern in item.DeniedUrls)
                {
                    if (IsMatchPattern(url, pattern))
                        return false;
                }
            }

            if (item.AllowedUrls != null)
            {
                foreach (var pattern in item.AllowedUrls)
                {
                    if (IsMatchPattern(url, pattern))
                        return true;
                }

                if (item.AllowedUrls.Any())
                    return false;
            }           

            return true;
        }

        protected virtual bool IsMatchPattern(Url url, string pattern)
        {
            Throws.IfArgumentNullOrEmpty(pattern, _ => pattern);

            if (pattern.EndsWith("*"))
            {
                var p = pattern.TrimEnd('*').TrimEnd('/').TrimStart('/');
                return url.Path.TrimEnd('/').TrimStart('/').ToLower().StartsWith(p.ToLower());
            }
            else
            {
                var u = url.Path.TrimEnd('/').TrimStart('/');
                var p = pattern.TrimEnd('/').TrimStart('/');

                return u.Equals(p, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
