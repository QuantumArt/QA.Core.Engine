using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Engine.Interface
{
    public interface IRegionHierarchyProvider
    {
        HierarchyRegion[] GetParentRegionsAndSelf(string alias);
    }

}
