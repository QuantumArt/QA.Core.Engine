using System.Collections.Generic;
using System.IO;
using System.Web;
using QA.Core.Engine.Web;
using QA.Core.Web;

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Утилиты для рендеринга управления виджетами
    /// </summary>
    public class PartUtilities
    {
        public const string TypeAttribute = "data-type";
        public const string TitleAttribute = "data-title";
        public const string IconUrlAttribute = "data-icon-url";
        public const string TemplateAttribute = "data-template";
        public const string PathAttribute = "data-item";
        public const string ZoneAttribute = "data-zone";
        public const string ItemIdAttribute = "data-itemid";
        public const string AllowedAttribute = "data-allowed";
        public const string AllChildrenAttribute = "data-all-children";

        public const string EditingQueryKey = "editing";
        public const string OnscreenQueryKey = "onscreen";

        IEditUrlManager managementUrls;
        IDefinitionManager definitions;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="managementUrls"></param>
        /// <param name="definitions"></param>
        public PartUtilities(IEditUrlManager managementUrls, IDefinitionManager definitions)
        {
            this.managementUrls = managementUrls;
            this.definitions = definitions;
        }

        public static string GetAllowedNames(string zoneName, IEnumerable<ItemDefinition> definitions)
        {
            var allowedDefinitions = new List<string>();
            foreach (ItemDefinition potentialChild in definitions)
            {
                if (!potentialChild.IsPage)
                {
                    allowedDefinitions.Add(potentialChild.Discriminator);
                }
            }
            return string.Join(",", allowedDefinitions.ToArray());
        }

        public void WriteTitleBar(TextWriter writer, AbstractItem item, AbstractItem currentItem,string returnUrl)
        {
            AbstractItem currentPage = currentItem.ClosestPage();
            var definition = definitions.GetDefinition(item);

            writer.Write("<div class='titleBar ");
            writer.Write(GetItemClass(definition));
            writer.Write("'>");
           
            WriteTitle(writer, definition, item);

            WriteCommand(writer, "ред.", "command edit widget-edit-command",
                // Url.Parse(managementUrls.GetEditExistingItemUrl(item)).AppendQuery("returnUrl", returnUrl).Encode(),
                item.Id,
                "edit");
            WriteCommand(writer, "удалить", "command delete",
                // Url.Parse(managementUrls.GetDeleteUrl(item)).AppendQuery("returnUrl", returnUrl).Encode(),
                item.Id,
                "delete");

            if (!item.IsPublished)
            {
                writer.WriteLine("<span class='new'>&nbsp;</span>");
            }

            using (TagWrapper.Begin("div", writer, new { @class="info"}))
            {
                using (TagWrapper.Begin("div", writer, new { @class = "control-tip" }))
                {
                    if (currentPage != item.ClosestPage())
                    {
                        writer.Write("Унаследован");
                    }
                    else
                    {
                        writer.Write("Принадлежит данной странице");
                    }
                }
                using (TagWrapper.Begin("div", writer, new { @class = "info-header" }))
                {
                    writer.Write(TagWrapper.NoBreakSpace);
                }
            }

            writer.Write("</div>");
        }

        private static string GetItemClass(ItemDefinition definition)
        {
            return "definition-" + (definition.Discriminator ?? "unknown");
        }

        public void WriteControlPanel(TextWriter writer, AbstractItem item, string currentUrl)
        {
            writer.Write("<div>");
            WriteCommand(writer, "ред. страницу", "command edit page-edit-command",
                item.Id, "edit");
            

            // удаляем ключи авторизации custom action QP8
            //backend_sid=59cbd0da-dd58-4314-919d-6860d7f3a064&amp;site_id=35&amp;customerCode=qp_beeline_main_dev&amp;hostUID
            WriteCommand(writer, "ред. виджеты", "command organize",
                Url.Parse(currentUrl ?? item.Url)
                .RemoveQuery("backend_sid")
                .RemoveQuery("hostUID")
                .AppendQuery(EditingQueryKey, true));
                                    
            WriteCommand(writer, "", "command refresh",
                Url.Parse(currentUrl)
                .RemoveQuery("backend_sid")
                .RemoveQuery("hostUID"));
                        
            writer.Write("</div>");
        }

        public static void WriteCommand(TextWriter writer, string title, string @class, string url)
        {
            using (TagWrapper.Begin("span", writer, null))
            {
                using (TagWrapper.BeginLink(writer, url, new { @class, title }))
                {
                    writer.Write(title);
                }
            }
        }

        public static void WriteCommand(TextWriter writer, string title, string @class, int itemId, string action)
        {
            using (TagWrapper.Begin("span", writer, null))
            {
                writer.Write("<a title = '" + title + "' class='" + @class + "" + "' href='");

                writer.Write("'");
                if (itemId != 0)
                {
                    writer.Write(string.Format(" data-action = '{0}' data-itemId = '{1}'",
                        action, itemId));
                }

                writer.Write(">" + title + "&nbsp;" + "</a>");
            }
        }

        private static void WriteTitle(TextWriter writer, ItemDefinition definition, AbstractItem item)
        {
            using (TagWrapper.Begin("span", writer,
                new
                {
                    @class = "title",
                    style = string.Format("background:url({0}) no-repeat;", definition.IconUrl),
                    title = definition.Title
                }))
            {
                writer.Write(item.Name);
            }
        }
    }
}
