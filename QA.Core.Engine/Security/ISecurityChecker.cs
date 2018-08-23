using System.Security.Principal;
using System.Web;
using QA.Core.Web;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    public interface ISecurityChecker : IAdministrationSecurityChecker
    {
        bool CheckPermitions(AbstractItem item, IPrincipal user);
    }
}
