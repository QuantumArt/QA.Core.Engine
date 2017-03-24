using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Configuration;

namespace QA.Core.Engine.FakeImpl
{
    public class QPSettings
    {
        private static Nullable<int> _rootPageId;

        public static int RootPageId
        {
            get
            {
                if (_rootPageId.HasValue)
                {
                    return _rootPageId.Value;
                }
                return Settings.GetInt32ValueForCurrentProperty();
            }
            set
            {
                _rootPageId = value;
            }
        }
    }
}
