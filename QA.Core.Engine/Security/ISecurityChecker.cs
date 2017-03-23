using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using QA.Core.Web;

namespace QA.Core.Engine
{
    public interface ISecurityChecker : IAdministrationSecurityChecker
    {
        bool CheckPermitions(AbstractItem item, IPrincipal user);
        bool CheckAuthorization(HttpContextBase context);
    }
}
