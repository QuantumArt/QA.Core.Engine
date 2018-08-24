using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable 1591

namespace QA.Core.Engine.FakeImpl
{
    public class QPSecurityCheckerSingleDB : QA.Core.Web.Qp.QPSecurityCheckerSingleDB, ISecurityChecker
    {
        #region ISecurityChecker Members

        public bool CheckPermitions(AbstractItem item, System.Security.Principal.IPrincipal user)
        {
            return true;
        }

        #endregion
    }
}
