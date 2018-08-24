#pragma warning disable 1591

namespace QA.Core.Engine
{
    public interface IPathFinder
    {
        PathData GetPath(AbstractItem item, string remainingUrl);
    }
}
