using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Data
{
    public class FakeStartPageProvider : IStartPageProvider
    {
        private IEngine _engine;
        public FakeStartPageProvider(IEngine engine)
        {
            _engine = engine;
        }

        public int GetStartPageId()
        {
            return 1703;
        }

        protected int GetStartPageId(string currentDns)
        {
            return 1703;
        }

        public AbstractItem GetRootPage()
        {
            return _engine.Persister.Get(GetRootPageId());
        }

        public int GetRootPageId()
        {
            return 1796;
        }


        public int GetStartPageId(Url url)
        {
            return 1703;
        }


        public bool IsStartPage(int id)
        {
            return id == 1703;
        }

        public string GetBinding(int id)
        {
            return HttpContext.Current.Request.Url.Authority;
        }
    }
}
