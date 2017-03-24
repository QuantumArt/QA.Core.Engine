using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using QA.Core.Engine;
using QA.Core.Engine.Web.Mvc;
using System.Collections.Generic;
using QA.Core.Engine.Collections;
using QA.Core.Engine.Web;
using System.Web;
using QA.Core;
using QA.Core.Engine.Filters;
using System.Text;

namespace QA.Engine.Extensions.Html
{
    public class ZoneHelper : IHtmlString
    {
        protected HtmlHelper Html { get; set; }
        protected TagBuilder Wrapper { get; set; }
        protected string ZoneName { get; set; }
        protected AbstractItem CurrentItem;
        protected AbstractItem CurrentPage;
        protected AbstractItem StartPage;
        string _currentPageUrl;

        private ItemFilter _filter;
        private static ItemFilter _defaultFilter = new NullFilter();

        protected ItemFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        protected ITemplateRenderer Renderer
        {
            get
            {
                return RouteExtensions.ResolveService<ITemplateRenderer>(Html.ViewContext.RouteData);
            }
        }



        public ZoneHelper(HtmlHelper helper, string zoneName, AbstractItem currentItem, AbstractItem startPage, AbstractItem currentPage)
        {
            ZoneName = zoneName;
            CurrentItem = currentItem;
            StartPage = startPage;
            CurrentPage = currentPage;
            Html = helper;
            _filter = new NullFilter();


            if (Html.ViewContext.HttpContext.Request.IsAjaxRequest())
            {
                var referrer = Html.ViewContext.HttpContext.Request.UrlReferrer;
                if (referrer != null)
                {
                    _currentPageUrl = new Url(referrer.ToString()).Path;
                }
            }

            if (_currentPageUrl == null)
            {
                var r = HttpContext.Current.Items["TrailedUrl"] as Url;
                if (r != null)
                {
                    _currentPageUrl = r;
                }
                else
                {
                    Url url = Html.ViewContext.HttpContext.Request.Url.ToString();
                    _currentPageUrl = url.Path;
                }
            }
        }

        #region Fluent
        public ZoneHelper WrapIn(string tagName, object attributes)
        {
            Wrapper = new TagBuilder(tagName);
            Wrapper.MergeAttributes(new RouteValueDictionary(attributes));

            return this;
        }

        /// <summary>
        /// Применить указанные фильтры при выводе вилджетов на страницу.
        /// Данные фильтры не будут применяться в режиме редактирования виджетов
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public ZoneHelper WithFilters(params ItemFilter[] filters)
        {
            _filter = new AllFilter(filters);
            return this;
        }

        #endregion

        public override string ToString()
        {
            return ToHtmlString();
        }

        public virtual void Render()
        {
            Render(Html.ViewContext.Writer);
        }

        public virtual void Render(TextWriter writer)
        {

            foreach (var child in GetItemsInZone().ToArray())
            {
                RenderTemplate(writer, child);
            }

        }

        public static void SetDefaultFilter(ItemFilter defaultFilter)
        {
            _defaultFilter = defaultFilter;
        }

        protected virtual IEnumerable<AbstractItem> GetItemsInZone()
        {
            return GetItemsInZoneInternal().OrderBy(x => x.SortOrder);
        }

        private IEnumerable<AbstractItem> GetItemsInZoneInternal()
        {
            var regionFilter = GetFilter();
            if (StartPage != null)
            {
                if (ZoneName.StartsWith("Site"))
                {
                    return StartPage.GetChildren(
                        new AllFilter(_filter,
                            _defaultFilter,
                            new IsPartFilter(),
                            regionFilter)
                        )
                        .Where(x => ZoneName.Equals(x.ZoneName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (CurrentItem != null)
            {
                if (ZoneName.StartsWith("Recursive"))
                {
                    // Логика работы рекурсивной зоны
                    AbstractItem node = CurrentItem;
                    List<AbstractItem> items = new List<AbstractItem>();
                    IUniqueInZone uniqueItem = null;
                    do
                    {
                        items.AddRange(node.GetChildren(
                            new AllFilter(_filter,
                                _defaultFilter,
                                new IsPartFilter(),
                                new DelegateFilter(x =>
                                {
                                    if (ZoneName.Equals(x.ZoneName, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (x is IUniqueInZone)
                                        {
                                            if (uniqueItem != null)
                                            {
                                                return false;
                                            }
                                            uniqueItem = (IUniqueInZone)x;
                                        }

                                        return true;
                                    }
                                    return false;
                                }),
                                regionFilter)
                            ));
                        node = node.Parent;
                    }
                    while (node != null);

                    return items;
                }

                return CurrentItem.GetChildren(new AllFilter(_filter, _defaultFilter, new IsPartFilter(), regionFilter)).Where(x => ZoneName.Equals(x.ZoneName, StringComparison.InvariantCultureIgnoreCase));
            }

            return new AbstractItem[] { };
        }

        private ItemFilter GetFilter()
        {
            var resolver = ObjectFactoryBase.Resolve<ICultureUrlResolver>();

            ItemFilter f = new NullFilter();

            if (_currentPageUrl != null)
            {
                f = new UrlPartFilter(_currentPageUrl);
            }

            return new AllFilter(f,
                new VersioningFilter(resolver.GetCurrentRegion(), resolver.GetCurrentCulture(), false));

        }

        protected virtual void RenderTemplate(TextWriter writer, AbstractItem model)
        {
            if (Wrapper != null)
                writer.Write(Wrapper.ToString(TagRenderMode.StartTag));

            Renderer.RenderTemplate(model, Html, writer);

            if (Wrapper != null)
                writer.WriteLine(Wrapper.ToString(TagRenderMode.EndTag));
        }

        public string ToHtmlString()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine(string.Format("<!-- The beginning of the \'{0}\' zone. -->", ZoneName));
                Render(writer);
                writer.WriteLine(string.Format("<!-- The end of the \'{0}\' zone. -->", ZoneName));
                return sb.ToString();
            }
        }
    }
}
