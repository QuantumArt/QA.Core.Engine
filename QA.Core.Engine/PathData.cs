// Owners: Karlov Nikolay

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Routing;

namespace QA.Core.Engine
{
    [Serializable, DebuggerDisplay("PathData ({CurrentItem})")]
    public class PathData
    {
        #region Static
        public const string DefaultAction = "";

        public static PathData Empty
        {
            get { return new PathData(); }
        }

        public static PathData None(AbstractItem reportedBy, string remainingUrl)
        {
            return new PathData { StopItem = reportedBy, Argument = remainingUrl };
        }

        public static PathData NonRewritable(AbstractItem item)
        {
            return new PathData(item, null) { IsRewritable = false };
        }

        /// <summary>
        /// Параметр query для ID произвольного элемента (страницы или виджета)
        /// </summary>
        static string itemQueryKey = "ui-item";

        /// <summary>
        /// Параметр query для ID страницы
        /// </summary>
        static string pageQueryKey = "ui-page";

        /// <summary>
        /// Параметр query для ID виджета
        /// </summary>
        static string partQueryKey = "ui-part";

        /// <summary>
        /// Параметр query для языка виджета
        /// </summary>
        static string cultureQueryKey = "ui-culture";

        static string selectedQueryKey = "ui-selected";

        /// <summary>
        /// Параметр query для региона виджета
        /// </summary>
        static string regionQueryKey = "ui-region";

        /// <summary>
        /// Параметр query для языка виджета
        /// </summary>
        public static string CultureQueryKey
        {
            get { return PathData.cultureQueryKey; }
            set { PathData.cultureQueryKey = value; }
        }

        /// <summary>
        /// Параметр query для языка виджета
        /// </summary>
        public static string RegionQueryKey
        {
            get { return PathData.regionQueryKey; }
            set { PathData.regionQueryKey = value; }
        }


        /// <summary>
        /// Параметр query для ID произвольного элемента (страницы или виджета)
        /// </summary>
        public static string ItemQueryKey
        {
            get { return itemQueryKey; }
            set { itemQueryKey = value; }
        }

        /// <summary>
        /// Параметр query для ID страницы
        /// </summary>
        public static string PageQueryKey
        {
            get { return pageQueryKey; }
            set { pageQueryKey = value; }
        }

        /// <summary>
        /// Параметр query для ID виджета
        /// </summary>
        public static string PartQueryKey
        {
            get { return partQueryKey; }
            set { partQueryKey = value; }
        }

        public static string SelectedQueryKey
        {
            get { return PathData.selectedQueryKey; }
            set { PathData.selectedQueryKey = value; }
        }
        #endregion

        AbstractItem currentPage;
        AbstractItem currentItem;

        private readonly RouteData _routeData;

        /// <summary>
        ///
        /// </summary>
        /// <param name="routeData">значения маршрутизации, полученные у innerRoute</param>
        public PathData(AbstractItem item, string templateUrl, RouteData routeData)
        {
            _routeData = routeData;
            if (item != null)
            {
                CurrentItem = item;
                Id = item.Id;
            }
            TemplateUrl = templateUrl;
            Action = routeData.Values["action"].ToString();
        }

        public PathData(AbstractItem item, string templateUrl, string action, string arguments)
            : this()
        {
            if (item != null)
            {
                CurrentItem = item;
                Id = item.Id;
            }
            TemplateUrl = templateUrl;
            Action = action;
            Argument = arguments;
        }

        public PathData(AbstractItem item, string templateUrl)
            : this(item, templateUrl, DefaultAction, string.Empty)
        {
        }

        public PathData(int id, int pageId, string path, string templateUrl, string action, string arguments, bool ignore, IDictionary<string, string> queryParameters)
            : this()
        {
            Id = id;
            PageId = pageId;
            Path = path;
            TemplateUrl = templateUrl;
            Action = action;
            Argument = arguments;
            Ignore = ignore;
            QueryParameters = new Dictionary<string, string>(queryParameters);
        }

        public PathData()
        {
            QueryParameters = new Dictionary<string, string>();
            IsRewritable = true;
            IsCacheable = true;
        }


        public AbstractItem CurrentItem
        {
            get { return currentItem; }
            set
            {
                currentItem = value;
                Id = value != null ? value.Id : 0;
            }
        }

        public AbstractItem CurrentPage
        {
            get { return currentPage ?? CurrentItem; }
            set
            {
                currentPage = value;
                PageId = value != null ? value.Id : 0;
            }
        }

        public AbstractItem StopItem { get; set; }

        public string TemplateUrl { get; set; }

        public int Id { get; set; }

        public int PageId { get; set; }

        public string Path { get; set; }

        public string Action { get; set; }

        public string Argument { get; set; }

        public IDictionary<string, string> QueryParameters { get; set; }

        public bool Ignore { get; set; }

        public bool IsRewritable { get; set; }

        public bool IsCacheable { get; set; }

        public virtual Url RewrittenUrl
        {
            get
            {
                if (IsEmpty() || string.IsNullOrEmpty(TemplateUrl))
                    return null;

                if (CurrentPage.IsPage)
                {
                    Url url = Url.Parse(TemplateUrl)
                        .UpdateQuery(QueryParameters)
                        .SetQueryParameter(PathData.PageQueryKey, CurrentPage.Id);
                    if (!string.IsNullOrEmpty(Argument))
                        url = url.SetQueryParameter("argument", Argument);

                    return url.ResolveTokens();
                }
                // TODO:  проверить корректность для контентных версий!

                for (AbstractItem ancestor = CurrentItem.Parent; ancestor != null; ancestor = ancestor.Parent)
                    if (ancestor.IsPage)
                        return ancestor.FindPath(DefaultAction, RegionToken, CultureToken).RewrittenUrl
                            .UpdateQuery(QueryParameters)
                            .SetQueryParameter(PathData.ItemQueryKey, CurrentItem.Id);

                if (CurrentItem.VersionOf != null)
                    return CurrentItem.VersionOf.FindPath(DefaultAction, RegionToken, CultureToken).RewrittenUrl
                        .UpdateQuery(QueryParameters)
                        .SetQueryParameter(PathData.ItemQueryKey, CurrentItem.Id);

                throw new Exception("TemplateNotFound " + CurrentItem.ToString());
            }
        }

        public virtual PathData UpdateParameters(IDictionary<string, string> queryString)
        {
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                if (string.Equals(pair.Key, "argument"))
                    Argument = pair.Value;
                else
                    QueryParameters[pair.Key] = pair.Value;
            }

            return this;
        }


        public virtual PathData Detach()
        {
            PathData data = MemberwiseClone() as PathData;

            data.currentItem = null;
            data.currentPage = null;
            data.StopItem = null;
            return data;
        }

        /// <summary>
        /// Создает копию для данного реквеста
        /// </summary>
        public virtual PathData Attach(IPersister persister)
        {
            PathData data = MemberwiseClone() as PathData;

            data.QueryParameters = new Dictionary<string, string>(QueryParameters);
            data.CurrentItem = persister.Repository.Load(Id);
            if (PageId != 0)
                data.CurrentPage = persister.Repository.Load(PageId);

            return data;
        }

        public virtual PathData SetArguments(string argument)
        {
            Argument = argument;
            return this;
        }

        public virtual bool IsEmpty()
        {
            return CurrentItem == null;
        }

        public string CultureToken { get; set; }

        public string RegionToken { get; set; }

        public Url TrailedUrl { get; set; }

        public RouteData RouteData
        {
            get
            {
                return _routeData;
            }
        }
    }
}
