using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Engine.FakeImpl
{
    public class UrlMatchingResult
    {
        private static readonly UrlMatchingResult _empty = new UrlMatchingResult()
        {
        };

        public static UrlMatchingResult Empty
        {
            get
            {
                return new UrlMatchingResult();
            }
        }

        public UrlMatchingResult()
        {

        }

        public bool IsMatch
        {
            get;
            set;
        }

        public string Region { get; set; }
        public string Culture { get; set; }
        public UrlMatchingPattern Pattern { get; set; }
        public Url SanitizedUrl { get; set; }
    }
}
