using System.Web.Mvc;
using System.Web;
using QA.Core.Engine;
using QA.Core;
using QA.Core.Engine.Web.Mvc;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace QA.Engine.Extensions.Html
{
    public static class ZoneExtensions
    {
        /// <summary>
        /// Шаблон для замены зон в тексте
        /// Пример: [[zone=имязоны]]
        /// </summary>
        public static string ZonePattern = @"\[\[zone=(\w+)\]\]";

        static Regex regex = new Regex(ZonePattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            return new WidgetZoneHelper(helper, zoneName,
                (AbstractItem)helper.GetCurrentItem(),
                (AbstractItem)helper.GetStartPage(),
                (AbstractItem)helper.GetCurrentPage());
        }

        public static WidgetZoneHelper WidgetZone(this HtmlHelper helper, AbstractItem item, string zoneName)
        {
            return new WidgetZoneHelper(helper, zoneName, item,
                (AbstractItem)helper.GetStartPage(),
                (AbstractItem)helper.GetCurrentPage());
        }

        /// <summary>
        /// Получить текущую стартовую страницу
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static AbstractItem GetStartPage(this HtmlHelper helper)
        {
            return (AbstractItem)helper.ViewContext.ViewData[ContentRoute.StartPageKey]
                ?? helper.ViewContext.RequestContext.StartPage();
        }

        /// <summary>
        /// Получить текущую страницу или виджет
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static AbstractItem GetCurrentItem(this HtmlHelper helper)
        {
            return (AbstractItem)helper.ViewContext.ViewData[ContentRoute.AbstractItemKey]
                ?? helper.ViewContext.RequestContext.CurrentItem();
        }

        /// <summary>
        /// Получить текущую страницу
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static AbstractItem GetCurrentPage(this HtmlHelper helper)
        {
            return (AbstractItem)helper.ViewContext.ViewData[ContentRoute.ContentPageKey]
                ?? helper.ViewContext.RequestContext.CurrentPage();
        }

        /// <summary>
        /// Рендеринг зон, объявлнных в контенте в виде [[zone=имя_зоны]]
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IHtmlString RenderZonesInText(this HtmlHelper helper, string text)
        {
            if (!regex.IsMatch(text))
            {
                return new MvcHtmlString(text);
            }

            var result = regex.Replace(text, (MatchEvaluator)(match =>
            {
                if (match.Groups.Count > 1)
                {
                    var group = match.Groups[1];
                    if (group.Success)
                    {
                        var zoneName = group.Value;

                        if (!string.IsNullOrEmpty(zoneName))
                        {
                            return helper
                                .WidgetZone(zoneName)
                                .ToString();
                        }
                    }
                }
                return string.Empty;
            }));

            return MvcHtmlString.Create(result);
        }
    }
}
