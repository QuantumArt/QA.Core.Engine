using System.Collections.Generic;
using System.Windows.Markup;
#pragma warning disable 1591

namespace QA.Core.Engine.FakeImpl
{
    [ContentProperty("MatchingPatterns")]
    public class CultureUrlParserConfig
    {
        public List<UrlMatchingPattern> MatchingPatterns { get; set; }

        public CultureUrlParserConfig()
        {
            MatchingPatterns = new List<UrlMatchingPattern>();
        }
    }

    [ContentProperty("Value")]
    public class UrlMatchingPattern
    {
        public int CultureTokenPosition;
        public int RegionTokenPosition;
        /// <summary>
        /// {region}.bee.ru/{culture}
        /// bee.ru/{culture}/{region}
        /// </summary>
        public string Value { get; set; }
        public bool ProcessCultureTokens { get; set; }
        public bool ProcessRegionTokens { get; set; }
        public string DefaultCultureToken { get; set; }
        public bool IsRegionInAuthority { get; set; }
        public bool UseForReplacing { get; set; }
        public bool IsCultureInAuthority { get; set; }
    }
}
