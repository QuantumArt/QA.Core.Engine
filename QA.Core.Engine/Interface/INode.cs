using System.Security.Principal;

namespace QA.Core.Engine
{
    public interface INode : ILink
    {
        string Name { get; }

        string Path { get; }

        string GetPreviewUrl();

        string IconUrl { get; }

        string ClassNames { get; }

        bool IsAuthorized(IPrincipal user);
    }
}
