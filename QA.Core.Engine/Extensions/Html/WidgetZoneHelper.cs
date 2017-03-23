using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using QA.Core.Engine;
using QA.Core.Engine.Collections;
using QA.Core.Engine.UI;
using QA.Core.Engine.Web;
using QA.Core.Engine.Web.Mvc;
using QA.Core.Web;

namespace QA.Engine.Extensions.Html
{
    public class WidgetZoneHelper : ZoneHelper
    {
        private System.Web.Mvc.HtmlHelper _helper;
        private ZoneState _zoneState;
        private string ZoneTitle;
        private const string AllZones = "";

        public WidgetZoneHelper(HtmlHelper helper, string name, AbstractItem currentItem, AbstractItem startPage, AbstractItem currentPage)
            : base(helper, name, currentItem, startPage, currentPage)
        {
            _helper = helper;
            _zoneState = ZoneExtensions.GetZoneState(helper.ViewContext.RequestContext.HttpContext);
        }

        #region Fluent

        public ZoneHelper Title(string title)
        {
            ZoneTitle = title;

            return this;
        }

        public override void Render(TextWriter writer)
        {
            if (_zoneState == ZoneState.Editing)
            {
                Filter = new NullFilter();
                if (ZoneName.IndexOfAny(new[] { '.', ',', ' ', '\'', '"', '\t', '\r', '\n' }) >= 0)
                    throw new Exception("Zone '" + ZoneName + "' contains illegal characters.");

                if (CurrentItem != null)
                {
                    writer.Write("<div class='" + ZoneName + " dropZone'");
                    writer.WriteAttribute(PartUtilities.PathAttribute, CurrentItem.Id.ToString())
                        .WriteAttribute(PartUtilities.ZoneAttribute, ZoneName)
                        .WriteAttribute(PartUtilities.AllowedAttribute,
                                        PartUtilities.GetAllowedNames(ZoneName,
                                            DefinitionsHelper.GetAllowedDefinitions(CurrentItem,
                                                ZoneName, _helper.ViewContext.HttpContext.User)))
                        .WriteAttribute(PartUtilities.AllowedAttribute, AllZones)
                        .WriteAttribute("title", ZoneTitle)
                        .Write(">");

                    writer.WriteLine(string.Format("<i>{0}</i>", ZoneName));

                    if (string.IsNullOrEmpty(Html.ViewContext.HttpContext.Request["preview"]))
                    {
                        base.Render(writer);
                    }
                    else
                    {
                        string preview = Html.ViewContext.HttpContext.Request["preview"];
                        RenderReplacingPreviewed(writer, preview);
                    }

                    //using (TagWrapper.Begin("div", writer, new { @class = "zone-item-wrapper" }))
                    //{
                    //    using (TagWrapper.Begin("div", writer, new { @class = "zone-item-helper" }))
                    //    {

                    //    }
                    //}
                    writer.Write("</div>");
                }
            }
            else
            {
                base.Render(writer);
            }

        }

        protected void RenderReplacingPreviewed(TextWriter writer, string preview)
        {
            int itemID;
            if (int.TryParse(preview, out itemID))
            {
                AbstractItem previewedItem = RouteExtensions.ResolveService<IPersister>(Html.ViewContext.RouteData).Get(itemID);
                if (previewedItem != null && previewedItem.VersionOf != null)
                {
                    foreach (var child in GetItemsInZone().ToArray())
                    {
                        if (previewedItem.VersionOf == child)
                        {
                            RenderTemplate(writer, previewedItem);
                        }
                        else
                        {
                            RenderTemplate(writer, child);
                        }
                    }

                }
            }
        }


        protected override void RenderTemplate(TextWriter writer, AbstractItem model)
        {
            if (_zoneState == ZoneState.Editing)
            {
                ItemDefinition definition = RouteExtensions.ResolveService<IDefinitionManager>(Html.ViewContext.RouteData).GetDefinition(model);

                using (TagWrapper.Begin("div", writer, new { @class = "zone-item-wrapper", beforeItem = model.Id }))
                {
                    writer.Write("<div class='" + definition.Discriminator + " zoneItem'");
                    writer.WriteAttribute(PartUtilities.PathAttribute, model.Path)
                        .WriteAttribute(PartUtilities.TypeAttribute, definition.Discriminator)
                        .WriteAttribute(PartUtilities.ItemIdAttribute, model.Id.ToString())
                        // список дочерних виджетов
                        .WriteAttribute(PartUtilities.AllChildrenAttribute,
                            string.Join(",",
                                model.GetChildrenRecursive()
                                .Select(x => x.Id)))
                        .Write(">");

                    RouteExtensions.ResolveService<PartUtilities>(Html.ViewContext.RouteData)
                        .WriteTitleBar(writer, model, CurrentItem, Html.ViewContext.HttpContext.Request.RawUrl);

                    writer.Write("<div class='widget-element'>");

                    base.RenderTemplate(writer, model);

                    writer.Write("</div>");
                    writer.Write("</div>");
                }
            }
            else
            {
                base.RenderTemplate(writer, model);
            }
        }

        #endregion
    }

    internal static class TextWriterExtensions
    {
        public static TextWriter WriteAttribute(this TextWriter writer, string attributeName, string value)
        {
            writer.Write(" {0}=\"{1}\"", attributeName, value);
            return writer;
        }
    }
}
