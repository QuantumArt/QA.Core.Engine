using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Engine
{
    public class RegionCollection : List<Region>
    {
        public static RegionCollection AllRegions = new RegionCollection();
    }
}
