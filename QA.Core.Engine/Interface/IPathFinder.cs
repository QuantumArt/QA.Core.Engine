
namespace QA.Core.Engine
{
    public interface IPathFinder
    {
        PathData GetPath(AbstractItem item, string remainingUrl);
    }
}
