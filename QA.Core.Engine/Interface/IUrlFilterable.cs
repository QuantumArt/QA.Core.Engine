
using System.Collections.Generic;
#pragma warning disable 1591

namespace QA.Core.Engine.Interface
{
    public interface IUrlFilterable
    {
        IEnumerable<string> AllowedUrls { get; }
        IEnumerable<string> DeniedUrls { get; }
    }
}
