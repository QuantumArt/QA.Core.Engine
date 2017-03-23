using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using QA.Core;
using QA.Core.Engine;
using QA.Core.Engine.Collections;
using QA.Core.Engine.Web;

namespace QA.Engine.Extensions.Html
{
    public class MenuHtmlHelper
    {
        private AbstractItem _currentItem;
        private AbstractItem _startPage;
        private HtmlHelper _helper;

        public MenuHtmlHelper(HtmlHelper helper, AbstractItem currentItem, AbstractItem startPage)
        {
            _currentItem = currentItem;
            if (_currentItem != null)
            {
                _currentItem = _currentItem.ClosestPage();
            }
            _startPage = startPage;
            _helper = helper;
        }

        public MvcHtmlString SiteMap(int startLevel, int endLevel, object htmlArguments = null)
        {
            return MvcHtmlString.Create(Render(startLevel, endLevel, htmlArguments));
        }

        public MvcHtmlString TopMenu(object htmlArguments = null, string @class = "")
        {
            return MvcHtmlString.Create(Render(1, 2, htmlArguments, @class));
        }

        public MvcHtmlString SubMenu(int start, string @class = "")
        {
            return MvcHtmlString.Create(RenderBranch(start, @class));
        }

        public MvcHtmlString Breadscrumb()
        {
            return MvcHtmlString.Create(BuildUp(_currentItem));
        }

        private string BuildUp(AbstractItem contentItem)
        {
            List<AbstractItem> tree = new List<AbstractItem>();
            AbstractItem item = contentItem;
            StringBuilder sb = new StringBuilder();
            while (item != null)
            {
                if (item is IRootPage)
                    break;

                tree.Add(item);
                item = item.Parent;
            }

            if (tree.Any())
            {
                // sb.AppendFormat("<span class=\"breadscrumb\">");

                for (int i = tree.Count - 1; i >= 0; i--)
                {
                    var node = tree[i];
                    if (i != 0)
                    {
                        sb.AppendLine("<li>");
                        sb.AppendFormat("<a href=\"{0}\" title=\"{1}\">{2}</a>", node.Url, node.Title, node.Title);
                        sb.AppendFormat("/");
                        sb.AppendLine("</li>");
                    }
                    else
                    {
                        //if (node.IsPage)
                        //{
                        //    sb.AppendFormat("<span title=\"{0}\">{1}</span>", node.Title, node.Title);
                        //}
                        //else
                        //{
                        //    sb.AppendFormat("<span>{0}</span>", _helper.ViewContext.RouteData.Values["action"]);
                        //}
                    }
                }

                //sb.AppendFormat("</span>");
            }

            return sb.ToString();
        }

        protected virtual string RenderBranch(int startLevel, string @class)
        {
            ItemFilter filter = new NavigationFilter();
            var sb = new StringBuilder();
            if (_currentItem != null && _startPage != null)
            {
                if (_currentItem != _startPage)
                {
                    var node = _currentItem;
                    var list = new List<AbstractItem>();

                    while (node != null)
                    {
                        if (node is IRootPage)
                            break;

                        list.Add(node);
                        node = node.Parent;
                    }
                    list.Reverse();
                    if (list.Count > startLevel)
                    {
                        var root = list[startLevel];
                        sb.AppendFormat("<h4><a href='{0}'>{1}</a></h4>", root.Url, root.Title);
                        sb.AppendLine("<div>");
                        sb.AppendFormat("<ul class='{0}'>", @class ?? string.Empty);

                        foreach (var item in root.GetChildren(GetFilter()))
                        {
                            if (filter.Match(item))
                            {
                                sb.AppendLine("<li>");
                                sb.AppendLine(string.Format("<a href=\"{1}\" class=\"{3}\" title=\"{2}\">{0}</a>",
                                    item.Title, item.Url, item.Title,
                                    item.Id == _currentItem.Id ? "selected" : "not-selected"));
                                sb.AppendLine("</li>");
                            }
                        }

                        sb.AppendLine("</ul>");
                        sb.AppendLine("</div>");
                    }
                }
            }

            return sb.ToString();
        }

        private ItemFilter GetFilter()
        {
            var resolver = ObjectFactoryBase.Resolve<ICultureUrlResolver>();
            return new VersioningFilter(resolver.GetCurrentRegion(), resolver.GetCurrentCulture());
        }

        protected virtual string Render(int startLevel, int endLevel, object htmlArguments, string @class = "")
        {
            StringBuilder sb = new StringBuilder();
            var item = _startPage;
            var level = 0;

            var canWrite = level >= startLevel;

            if (canWrite)
            {
                OpenList(sb, level, level == startLevel ? @class : "");
            }

            WriteLevel(sb, item, startLevel, endLevel, level);

            if (canWrite)
            {
                CloseList(sb);
            }
            return sb.ToString();
        }

        private void WriteLevel(StringBuilder sb, AbstractItem item, int startLevel, int maxLevel, int level, string @class = "")
        {
            if (maxLevel != 0 && level >= maxLevel)
            {
                return;
            }

            var canWrite = level >= startLevel;
            var canWriteChildren = level < maxLevel - 1;

            level += 1;

            if (_startPage != null)
            {
                if (canWrite)
                {
                    var currentPage = _currentItem != null ? (_currentItem.ClosestPage() ?? _currentItem) : null;
                    sb.AppendLine(string.Format("<li class=\"{1}\" itemId=\"{0}\">", item.Id, (currentPage != null && item.Id == currentPage.Id) ? "active" : ""));

                    sb.AppendLine(string.Format("<a href=\"{1}\" itemId=\"{5}\" class=\"{3} {4}\" title=\"{2}\">{0}</a>",
                        item.Title, item.Url, item.Title,
                        (currentPage != null && item.Id == currentPage.Id) ? "selected" : "not-selected",
                        item.IsPublished ? "published" : "not-published", item.Id));

                }
                var children = item.GetChildren(GetFilter());
                if (children.Any() && canWriteChildren)
                {
                    if (canWrite)
                    {
                        OpenList(sb, level, level == startLevel + 1 ? @class : "");
                    }

                    foreach (var child in children)
                    {
                        WriteLevel(sb, child, startLevel, maxLevel, level);
                    }

                    if (canWrite)
                    {
                        CloseList(sb);
                    }
                }

                if (canWrite)
                {
                    sb.AppendLine("</li>");
                }
            }
        }

        private static void CloseList(StringBuilder sb)
        {
            sb.AppendLine("</ul>");
        }

        private static void OpenList(StringBuilder sb, int level, string @class = "")
        {
            sb.AppendLine(string.Format("<ul class={0}>", (level == 0 ? "root" : string.Empty) + " " + @class));
        }
    }
}
