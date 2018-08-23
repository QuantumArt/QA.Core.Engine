using System.Collections.Generic;
using System.Linq;
#pragma warning disable 1591

namespace QA.Core.Engine.Collections
{

    public interface IZonedList<T> where T : class, IPlaceable
    {
        IQueryable<T> FindNavigatablePages();
        IQueryable<T> FindPages();
        IQueryable<T> FindParts();
        IQueryable<T> FindParts(string zoneName);
        IEnumerable<string> FindZoneNames();
    }
}
