
using System.Collections.Generic;
namespace QA.Core.Engine.Interface
{
    public interface IUrlFilterable
    {
        IEnumerable<string> AllowedUrls { get; }
        IEnumerable<string> DeniedUrls { get; }
    }
}
