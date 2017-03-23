using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
    public interface ITemplateRenderer
    {
        /// <summary>
        /// Рендерить виджет в текущий поток
        /// </summary>
        /// <param name="item">виджект</param>
        /// <param name="helper"></param>
        void RenderTemplate(AbstractItem item, HtmlHelper helper);

        /// <summary>
        /// Рендерить виджет в указанный поток
        /// </summary>
        /// <param name="item">виджект</param>
        /// <param name="helper"></param>
        /// <param name="writer">поток, в который происходит рендеринг виджета</param>
        void RenderTemplate(AbstractItem item, HtmlHelper helper, TextWriter writer);

        /// <summary>
        /// Получить контент виджета (аналогично RenderTemplate) без рендеринга в текущий поток
        /// </summary>
        /// <param name="item"></param>
        /// <param name="helper"></param>
        /// <returns>контент виджета</returns>
        IHtmlString GetTemplate(AbstractItem item, HtmlHelper helper);
    }

    public class TemplateRenderer : ITemplateRenderer
    {
        private readonly IControllerMapper controllerMapper;
        private ILogger _logger;

        public TemplateRenderer(IControllerMapper controllerMapper, ILogger logger)
        {
            _logger = logger;
            this.controllerMapper = controllerMapper;
        }

        public void RenderTemplate(AbstractItem item, HtmlHelper helper)
        {
            RenderTemplateInternal(item, helper, null);
        }

        public void RenderTemplate(AbstractItem item, HtmlHelper helper, TextWriter writer)
        {
            RenderTemplateInternal(item, helper, writer);
        }

        public IHtmlString GetTemplate(AbstractItem item, HtmlHelper helper)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                RenderTemplateInternal(item, helper, writer);
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        protected void RenderTemplateInternal(AbstractItem item, HtmlHelper helper, TextWriter writer)
        {
            Type itemType = item.GetContentType();
            string controllerName = controllerMapper.GetControllerName(itemType);
            if (string.IsNullOrEmpty(controllerName))
            {
                _logger.Error("Found no controller for type " + itemType);
                return;
            }

            RouteValueDictionary values = GetRouteValues(helper, item, controllerName);

            try
            {
                if (writer == null)
                {
                    helper.RenderAction("Index", values);
                }
                else
                {
                    writer.Write(helper.Action("Index", values));
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(string.Format(
                    "Tempalate render encountered problem with {0}", item),
                    ex);

                throw;
            }
        }

        private static RouteValueDictionary GetRouteValues(HtmlHelper helper, AbstractItem item, string controllerName)
        {
            var values = new RouteValueDictionary();
            values[ContentRoute.ControllerKey] = controllerName;
            values[ContentRoute.ActionKey] = "Index";
            values[ContentRoute.AbstractItemKey] = item; //item.Id;

            helper.ViewContext.HttpContext.Items["currentpartid"] = item.Id;
            // не будем поддерживать Areas
            //var vpd = helper.RouteCollection.GetVirtualPath(helper.ViewContext.RequestContext, values);
            //if (vpd == null)
            //    throw new InvalidOperationException("Unable to render " + item + " (" + values.ToQueryString() + " did not match any route)");

            //var areaToken = vpd.DataTokens["area"] as string;
            //values["area"] = areaToken ?? string.Empty;
            values["area"] = string.Empty;
            return values;
        }
    }
}
