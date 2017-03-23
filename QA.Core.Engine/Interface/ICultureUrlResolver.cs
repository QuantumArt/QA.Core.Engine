
namespace QA.Core.Engine.Web
{
    public interface ICultureUrlResolver
    {
        Url ResolveCulture(Url url, out string cultureToken, out string regionToken, bool replaceUrl);
        Url ResolveCultureReusable(Url url, out string cultureToken, out string regionToken, bool replaceUrl);
        Url AddTokensToUrl(Url url, string culture, string region);
        /// <summary>
        /// добавление или изменение токенов в адресе
        /// </summary>
        /// <param name="url"></param>
        /// <param name="culture"></param>
        /// <param name="region"></param>
        /// <param name="sanitizeUrl">true: очистить текущие токены из адреса, если они есть и тоько после этого добавить новые</param>
        /// <returns></returns>
        Url AddTokensToUrl(Url url, string culture, string region, bool sanitizeUrl);
        string GetCurrentCulture();
        string GetCurrentRegion();
        bool TrySetCulture(string token);
        bool TrySetRegion(string token);
        bool IsResolved { get; }
    }
}
