using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Web;

namespace QA.Core.Engine
{
    /// <summary>
    /// Версия провайдера, которая не определяет регион и культуру.
    /// </summary>
    public class EmptyCultureUrlResolver : ICultureUrlResolver
    {
        #region ICultureUrlResolver Members

        Url ICultureUrlResolver.ResolveCulture(Url url, out string cultureToken, out string regionToken, bool replaceUrl)
        {
            regionToken = null;
            cultureToken = GetCulture(); ;
            return url;
        }

        Url ICultureUrlResolver.ResolveCultureReusable(Url url, out string cultureToken, out string regionToken, bool replaceUrl)
        {
            regionToken = null;
            cultureToken = GetCulture();
            return url;
        }

        private static string GetCulture()
        {
            string cultureToken;
            cultureToken = CultureInfo.CurrentUICulture.ToString().ToLower();
            return cultureToken;
        }

        Url ICultureUrlResolver.AddTokensToUrl(Url url, string culture, string region)
        {
            return url;
        }

        Url ICultureUrlResolver.AddTokensToUrl(Url url, string culture, string region, bool sanitizeUrl)
        {
            return url;
        }

        string ICultureUrlResolver.GetCurrentCulture()
        {
            return GetCulture();
        }

        string ICultureUrlResolver.GetCurrentRegion()
        {
            return null;
        }

        bool ICultureUrlResolver.TrySetCulture(string token)
        {
            return true;
        }

        bool ICultureUrlResolver.TrySetRegion(string token)
        {
            return true;
        }

        bool ICultureUrlResolver.IsResolved
        {
            get { return true; }
        }

        #endregion
    }
}
