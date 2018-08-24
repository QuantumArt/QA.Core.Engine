using System.Web.Routing;
using System.Web.Mvc;
using QA.Core.Engine.Editing.Controllers;
#pragma warning disable 1591

namespace QA.Core.Engine.Editing
{
    public static class RouteTableExtensions
    {
        /// <summary>
        /// Установка маршрута для редактирования.
        /// Значение по умолчанию: "cms/managment/{action}"
        /// </summary>
        /// <param name="table"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Route MapEditingRoute(this RouteCollection table, string pattern = null)
        {
            pattern = pattern ?? "cms/managment/{action}";
            return table.MapRoute("Widget_EditingRoute", pattern,
                new
                {
                    controller = "WidgetManagment",
                    action = "Index"
                },
                new[] { typeof(WidgetManagmentController).Namespace });
        }
    }
}
