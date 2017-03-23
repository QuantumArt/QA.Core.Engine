using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Engine
{
    public class RegionCollection : List<Region>
    {
        public RegionCollection() : base() { }

        public RegionCollection(IEnumerable<Region> items) : base(items) { }
        public static RegionCollection AllRegions = new RegionCollection();
    }
}
