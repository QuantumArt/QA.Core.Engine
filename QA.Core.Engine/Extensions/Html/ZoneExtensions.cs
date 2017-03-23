using System.Web.Mvc;
using System.Web;
using QA.Core.Engine;
using QA.Core;

namespace QA.Engine.Extensions.Html
{
    public static class ZoneExtensions
    {
        public static ISecurityChecker SecurityChecker { get { return ObjectFactoryBase.Resolve<ISecurityChecker>(); } }

        public static ZoneState GetZoneState(this HttpContextBase context)
        {
            if (SecurityChecker.CheckAuthorization(context) == false)
            {
                return ZoneState.Disabled;
            }
            if (!string.IsNullOrEmpty(context.Request.QueryString["editing"]))
            {
                return ZoneState.Editing;
            }
            else
            {
                return ZoneState.Visible;
            }
        }

        public static ZoneState GetControlState(this HttpContextBase context)
        {
            var editingMode = !string.IsNullOrEmpty(context.Request.QueryString["editing"]);
            var allowed = SecurityChecker.CheckAuthorization(context);

            if (editingMode && allowed)
            {
                return ZoneState.Editing;
            }
            else if (!editingMode && allowed)
            {
                return ZoneState.Visible;
            }
            else
            {
                return ZoneState.Disabled;
            }
        }

        public static ZoneState GetOnScreenState(this HttpContextBase context)
        {
            var onscreenMode = !string.IsNullOrEmpty(context.Request.QueryString["onscreen"]);
            var allowed = SecurityChecker.CheckAuthorization(context);
            
            if (onscreenMode && allowed)
            {
                return ZoneState.OnScreen;
            }

            return ZoneState.Disabled;
        }

        public static ControlPanelHelper ControlPanel(this HtmlHelper helper)
        {
            return new ControlPanelHelper(helper, (AbstractItem)helper.ViewContext.ViewData[ContentRoute.AbstractItemKey]);
        }

        public static WidgetZoneHelper WidgetZone(this HtmlHelper helper, string zoneName)
        {
            return new WidgetZoneHelper(helper, zoneName, (AbstractItem)helper.ViewContext.ViewData[ContentRoute.AbstractItemKey],
                (AbstractItem)helper.ViewContext.ViewData[ContentRoute.StartPageKey],
                (AbstractItem)helper.ViewContext.ViewData[ContentRoute.ContentPageKey]);
        }

        public static WidgetZoneHelper WidgetZone(this HtmlHelper helper, AbstractItem item, string zoneName)
        {
            return new WidgetZoneHelper(helper, zoneName, item, 
                (AbstractItem)helper.ViewContext.ViewData[ContentRoute.StartPageKey],
                (AbstractItem)helper.ViewContext.ViewData[ContentRoute.ContentPageKey]);
        }
    }
}
