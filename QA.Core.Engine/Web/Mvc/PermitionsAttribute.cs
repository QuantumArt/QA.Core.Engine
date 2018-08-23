using System.Web.Mvc;
#pragma warning disable 1591

namespace QA.Core.Engine.Web.Mvc
{
    public class PermitionsAttribute : AuthorizeAttribute
    {
        public string RedirectUrl { get; set; }

        public static ISecurityChecker SecurityChecker { get { return ObjectFactoryBase.Resolve<ISecurityChecker>(); } }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return SecurityChecker.CheckAuthorization(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!string.IsNullOrEmpty(RedirectUrl))
            {
                filterContext.Result = new RedirectResult(RedirectUrl);
            }
        }
    }
}
