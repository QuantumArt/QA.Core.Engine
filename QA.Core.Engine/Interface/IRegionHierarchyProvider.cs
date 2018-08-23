#pragma warning disable 1591

namespace QA.Core.Engine.Interface
{
    public interface IRegionHierarchyProvider
    {
        HierarchyRegion[] GetParentRegionsAndSelf(string alias);
    }

}
