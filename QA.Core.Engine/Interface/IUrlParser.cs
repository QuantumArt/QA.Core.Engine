using System;
#pragma warning disable 1591

namespace QA.Core.Engine.Web
{
    public interface IUrlParser
    {

        AbstractItem StartPage { get; }
        AbstractItem RootPage { get; }

        AbstractItem CurrentPage { get; }

        string BuildUrl(AbstractItem item);

        string BuildUrl(AbstractItem item, string regionToken, string cultureToken);

        /// <summary>
        /// Добавление токенов в адрес
        /// </summary>
        /// <returns></returns>
        Url AddTokenToUrl(string url, string cultureToken, string regionToken);

        bool IsRootOrStartPage(AbstractItem item);

        PathData ResolvePath(Url url);
        PathData ResolvePath(Url url, bool reusable);

        AbstractItem Parse(string url);

        /// <summary>Removes a trailing Default.aspx from an URL.</summary>
        string StripDefaultDocument(string path);

        bool IsStartPage(AbstractItem current);
    }
}
