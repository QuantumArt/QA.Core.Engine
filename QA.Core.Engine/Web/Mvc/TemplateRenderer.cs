using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace QA.Core.Engine.Web.Mvc
{
    public interface ITemplateRenderer
    {
        void RenderTemplate(AbstractItem item, HtmlHelper helper);
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
                helper.RenderAction("Index", values);
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
