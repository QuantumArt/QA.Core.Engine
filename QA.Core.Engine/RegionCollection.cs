using System.Collections.Generic;

#pragma warning disable 1591

namespace QA.Core.Engine
{
    public class RegionCollection : List<Region>
    {
        public static RegionCollection AllRegions = new RegionCollection();
    }
}
