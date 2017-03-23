using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using QA.Core.Data.Collections;
using QA.Core.Engine.UI;
using QA.Core.Engine.Web;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Пакетный загрузчик всей структуры сайта.
    /// </summary>
    public class AbstractItemLoader
    {
        private readonly AbstractItemActivator _activator;
        private readonly IEngine _engine;
        private readonly IDefinitionManager _manager;

        private readonly static Regex FieldExpr = new Regex(@"^field_[0-9]+/", RegexOptions.Compiled);
        private readonly static Regex IdExpr = new Regex(@"[0-9]+", RegexOptions.Compiled);
        private readonly ConcurrentDictionary<Type, IReadOnlyList<ILoaderOption>> _needToResolve;

        static bool IsStage
        {
            get
            {
                bool value = false;

                bool.TryParse(ConfigurationManager.AppSettings["Mode.IsStage"] ?? "", out value);

                return value;
            }
        }

        /// <summary>
        /// Добавлять в коллекцию Details поля из таблицы AbstractItem
        /// </summary>
        static bool IncludeBaseFieldsIntoDetails
        {
            get
            {
                bool value = false;

                bool.TryParse(ConfigurationManager.AppSettings["IncludeBaseFieldsIntoDetails"] ?? "", out value);

                return value;
            }
        }

        public AbstractItemLoader(AbstractItemActivator activator, IEngine engine, IDefinitionManager manager)
        {
            _activator = activator;
            _engine = engine;
            _manager = manager;
            _needToResolve = new ConcurrentDictionary<Type, IReadOnlyList<ILoaderOption>>();
        }

        public void LoadAll(AbstractItemModel<int, AbstractItem> Model)
        {
            IUrlParser urlParser = null;
            ICultureUrlResolver cultureUrlresolver = null;

            try
            {
                urlParser = _engine.Resolve<IUrlParser>();
                cultureUrlresolver = _engine.Resolve<ICultureUrlResolver>();
            }
            catch (Exception ex)
            {
                _engine.Resolve<ILogger>().ErrorException("Resolving type in AbstractItemLoader is failed. ", ex);
            }

            var locker = Model.Locker;

            var newItems = new Dictionary<int, AbstractItem>();
            var groupedIds = new Dictionary<int, Dictionary<int, object>>();

            // process loading into newItems
            Stopwatch sw = new Stopwatch();

            sw.Start();

            #region loading
            var ctx = LinqHelper.Context;

            var loaded = ReadWithNoLock(
                () => (from t in ctx.QPAbstractItems
                       select new
                       {
                           Culture = t.Culture,
                           t.Discriminator_ID,
                           t.VersionOf_ID,
                           t.Id,
                           t.Parent_ID,
                           t.Name,
                           t.Title,
                           t.ContentId,
                           t.IndexOrder,
                           t.ExtensionId,
                           t.IsInSiteMap,
                           t.AuthenticationTargeting,
                           Targeting = t.Targeting,
                           StatusTypeName = t.StatusType.Name,
                           t.ZoneName,
                           t.Tags,
                           t.IsVisible,
                           t.MetaDescription,
                           TitleFormatId = t.TitleFormat_ID,
                           t.Created,
                           t.Modified,
                           t.Description,
                           t.Keywords,
                           t.AllowedUrlPatterns,
                           t.DeniedUrlPatterns
                       }).ToArray()
                          .GroupBy(x => x.Discriminator_ID));

            var timer_aiLoaded = sw.ElapsedMilliseconds;

            var regionMapping = ReadWithNoLock(
                () => ctx.AbstractItemAbtractItemRegionArticles
                    //.Where(x => x.QPRegion.Visible)
                    // не нужно, так как отсутствующие связи не загружаются.
                        .Select(x => new { x.QPAbstractItem_ID, x.QPRegion_ID })
                        .ToLookup(x => x.QPAbstractItem_ID, x => x.QPRegion_ID)
                    );

            var timer_rmLoaded = sw.ElapsedMilliseconds;

            var allRegions = ReadWithNoLock(
                () => ctx.QPRegions
                        .Select(x => new Region { Id = x.Id, Alias = x.Alias, Title = x.Title })
                        .ToLookup(x => x.Id, x => x)
                        );

            var timer_arLoaded = sw.ElapsedMilliseconds;
            #endregion

            // get discriminators
            // TODO: use services
            var discriminators = ctx.QPDiscriminators.Select(x => x)
                .ToArray()
                .ToDictionary(k => k.Id, v => v);

#if DEBUG
            var list = string.Join("\n", discriminators.Values.Select(x => x.Name + " " + x.IsPage));
#endif

            // process loaded data
            #region Processing
            foreach (var group in loaded)
            {
                if (group.Key == null || group.Key.Value <= 0)
                {
                    continue;
                }

                QPDiscriminator discriminator = null;

                if (discriminators.TryGetValue(group.Key.Value, out discriminator))
                {
                    // processing extensions
                    var contentId = discriminator.PreferredContentId ?? 0;
                    if (!groupedIds.ContainsKey(contentId))
                    {
                        groupedIds.Add(contentId, new Dictionary<int, object>());
                    }
                    var groupedItems = groupedIds[contentId];

                    foreach (var item in group)
                    {
                        if ((item.ExtensionId ?? 0) != contentId)
                        {
                            Debug.WriteLine(string.Format("Несоответствие расширения и расширения в PreferredContentId: {0}, {1} for {2} ({3})",
                                item.ExtensionId, contentId, item.Id, item.Name));
                        }


                        var newItem = _activator.CreateInstance(discriminator.Name, false);

                        if (newItem == null)
                        {
                            continue;
                        }

                        ((IInjectable<IUrlParser>)newItem).Set(urlParser);
                        ((IInjectable<ICultureUrlResolver>)newItem).Set(cultureUrlresolver);

                        // TODO: mapping

                        newItem.Regions = new RegionCollection(regionMapping[item.Id]
                            .Select(x => allRegions[x].FirstOrDefault())
                            .Where(x => x != null));

                        if (item.Culture != null)
                            newItem.Culture = new Culture { Id = item.Culture.Id, Key = item.Culture.Name, Name = item.Culture.Name, Title = item.Culture.Title };

                        newItem.Id = item.Id;
                        newItem.Name = item.Name;
                        newItem.Description = item.Description;
                        newItem.Keywords = item.Keywords;
                        newItem.Tags = item.Tags;
                        newItem.ParentId = item.Parent_ID ?? 0;
                        newItem.Title = item.Title;
                        newItem.Name = item.Name;
                        newItem.MetaDescription = item.MetaDescription;
                        newItem.VersionOfId = item.VersionOf_ID;
                        newItem.ExtensionId = item.ExtensionId;
                        newItem.TitleFormat = item.TitleFormatId ?? 0;
                        newItem.ZoneName = item.ZoneName;
                        newItem.IsVisible = item.IsVisible ?? false;
                        newItem.IsInSitemap = item.IsInSiteMap ?? false;
                        newItem.SortOrder = item.IndexOrder ?? 0;
                        newItem.IsPublished = item.StatusTypeName.Equals("published", StringComparison.InvariantCultureIgnoreCase);
                        newItems.Add(newItem.Id, newItem);
                        newItem.Created = item.Created;
                        newItem.Updated = item.Modified;

                        if (!string.IsNullOrEmpty(item.AuthenticationTargeting))
                        {
                            newItem.Access = string.Equals(item.AuthenticationTargeting, "Authencticated", StringComparison.InvariantCultureIgnoreCase)
                                ? AuthorizationTargeting.AuthorizedOnly :
                                    (string.Equals(item.AuthenticationTargeting, "NonAuthenticated", StringComparison.InvariantCultureIgnoreCase)
                                        ? AuthorizationTargeting.AnonymousOnly : AuthorizationTargeting.All);
                        }

                        newItem.Targeting = item.Targeting;

                        if (!string.IsNullOrWhiteSpace(item.AllowedUrlPatterns))
                        {
                            newItem.AllowedUrls = item.AllowedUrlPatterns.SplitString('\n', '\r', ';', ' ', ',');
                        }

                        if (!string.IsNullOrWhiteSpace(item.DeniedUrlPatterns))
                        {
                            newItem.DeniedUrls = item.DeniedUrlPatterns.SplitString('\n', '\r', ';', ' ', ',');
                        }

                        groupedItems.Add(newItem.Id, null);
                    }
                }
            }
            #endregion

            // Ключом служит идентификатор расширения, а не AbstractItem
            var needLoadM2MItems = new Dictionary<int, AbstractItem>();
            // process nodes' extensions

            int aiContentID = ctx.Cnn.GetContentIdByNetName(ctx.SiteId, typeof(QPAbstractItem).Name);

            foreach (var contentIdKey in groupedIds.Keys)
            {
                if ((!IncludeBaseFieldsIntoDetails && contentIdKey <= 0) || groupedIds[contentIdKey].Keys.Count == 0)
                    continue;

                ItemDefinition def = null;
                if (contentIdKey > 0)
                {
                    def = _manager.GetDefinitions()
                        .FirstOrDefault(x => x.PreferredContentId == contentIdKey);
                    if (def == null)
                    {
                        Trace.WriteLine(String.Format("Для contentIdKey '{0}' отсутствует ItemDefinition", contentIdKey));
                        continue;
                    }
                }

                var results = new DataTable();
                string conStr = ctx.ConnectionString;
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand sqlCmd = new SqlCommand("dbo.qa_beeline_extend_items", con);

                    sqlCmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = sqlCmd.Parameters
                        .AddWithValue("@Ids", groupedIds[contentIdKey].Keys.CreateSqlDataRecords().Ensure());

                    // TODO: get current state
                    SqlParameter isLive = sqlCmd.Parameters.AddWithValue("@isLive", !IsStage);
                    SqlParameter contentId = sqlCmd.Parameters.AddWithValue("@contentId", contentIdKey);

                    isLive.SqlDbType = SqlDbType.Bit;
                    contentId.SqlDbType = SqlDbType.Int;
                    tvpParam.SqlDbType = SqlDbType.Structured;

                    SqlParameter includeBaseFieldsParam = sqlCmd.Parameters.AddWithValue("@includeBaseFields", IncludeBaseFieldsIntoDetails);
                    includeBaseFieldsParam.SqlDbType = SqlDbType.Bit;

                    SqlParameter aiContentId = sqlCmd.Parameters.AddWithValue("@baseContentId", aiContentID);
                    aiContentId.SqlDbType = SqlDbType.Int;

                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        results.Load(reader);
                    }
                }
                IReadOnlyList<ILoaderOption> options = null;
                ILookup<string, ILoaderOption> optionsMap = null;
                for (int i = 0; i < results.Rows.Count; i++)
                {
                    var row = results.Rows[i];
                    var id = Convert.ToInt32(row["Id"]);
                    var item = newItems[id];

                    if (options == null)
                    {
                        options = ProcessType(item.GetContentType());
                        if (options.Count > 0)
                        {
                            optionsMap = options.ToLookup(x => x.PropertyName);
                        }
                    }

                    for (int j = 0; j < results.Columns.Count; j++)
                    {
                        var column = results.Columns[j];
                        var value = row[column];

                        if (value is string)
                        {
                            // process html replacing
                            var stringValue = value as string;
                            if (!string.IsNullOrWhiteSpace(stringValue))
                            {
                                if (optionsMap != null)
                                {
                                    foreach (var option in optionsMap[column.ColumnName])
                                    {
                                        stringValue = option.Process(new DBConnectorWrapper(ctx.Cnn), ctx, stringValue);
                                    }

                                    value = stringValue;
                                }
                                else
                                {
                                    // TODO: get attribute                                   
                                }

                                value = ctx.ReplacePlaceholders(stringValue);
                            }

                        }

                        item.Details.Add(column.ColumnName, value is DBNull ? null : value);

                        if (value is decimal
                            && def != null && def.NeedLoadM2MRelationsIds
                            && String.Compare(column.ColumnName, "CONTENT_ITEM_ID", true) == 0)
                        {
                            needLoadM2MItems.Add(item.GetDetail<int>("CONTENT_ITEM_ID", 0), item);
                        }
                    }
                }
            }

            var timer_exLoaded = sw.ElapsedMilliseconds;

            DoWithNoLock(() =>
            {
                LoadM2MRelationsIds(needLoadM2MItems);
            });

            var timer_m2mLoaded = sw.ElapsedMilliseconds;
            sw.Stop();

            ObjectFactoryBase.Resolve<ILogger>()
                .LogInfo(() => "Structure loaded in " + new
                {
                    timer_aiLoaded,
                    timer_rmLoaded,
                    timer_arLoaded,
                    timer_exLoaded,
                    timer_m2mLoaded
                });

            needLoadM2MItems = null;

            // process nodes' hierarchy
            foreach (var item in newItems.Values)
            {
                if (item.ParentId != null && item.ParentId > 0 && item.Parent == null)
                {
                    AbstractItem parent = null;
                    if (newItems.TryGetValue(item.ParentId.Value, out parent))
                    {
                        item.AttachTo(parent);
                    }
                }

            }

            foreach (var item in newItems.Values)
            {
                if (item.VersionOfId != null && item.VersionOfId > 0 && item.VersionOf == null)
                {
                    AbstractItem version = null;
                    if (newItems.TryGetValue(item.VersionOfId.Value, out version))
                    {
                        item.AttachVersionTo(version);
                    }
                }
            }

            lock (locker)
            {
                Model.Root = newItems.Values
                    .FirstOrDefault(x => x.ParentId == null
                        || x.ParentId == x.Id
                        || x.ParentId == default(int));

                if (Model.Root == null)
                    throw new InvalidOperationException("Обнаружены циклические ссылки в структуре сайта. Не найдена корневая страница.");

                Model.SetItems(newItems);
            }
        }

        protected void LoadM2MRelationsIds(Dictionary<int, AbstractItem> mapItemsToContentItemIDs)
        {
            SqlCommand cmd = new SqlCommand("dbo.qa_beeline_extend_items_m2m");
            cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter tvpParam = cmd.Parameters.AddWithValue("@Ids",
                mapItemsToContentItemIDs.Keys.CreateSqlDataRecords()
                .Ensure());

            SqlParameter isLiveParam = cmd.Parameters.AddWithValue("@isLive", !IsStage);
            isLiveParam.SqlDbType = SqlDbType.Bit;
            tvpParam.SqlDbType = SqlDbType.Structured;

            var results = new DataTable();

            DoWithNoLock(() =>
            {
                using (SqlConnection con = new SqlConnection(LinqHelper.Context.ConnectionString))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        results.Load(reader);
                    }
                }
            });

            foreach (DataRow result in results.Rows)
            {
                var itemId = Convert.ToInt32(result["item_id"]);
                var linkId = Convert.ToInt32(result["link_id"]);
                var linkedItemId = Convert.ToInt32(result["linked_item_id"]);
                mapItemsToContentItemIDs[itemId].AddRelation(linkId, linkedItemId);
            }
        }

        protected IReadOnlyList<ILoaderOption> ProcessType(Type t)
        {
            return _needToResolve.GetOrAdd(t, key =>
            {
                var lst = new List<ILoaderOption>();
                var properties = t.GetProperties();

                foreach (var prop in properties)
                {
                    var attrs = prop.GetCustomAttributes(typeof(ILoaderOption), false)
                        .Cast<ILoaderOption>();

                    foreach (var attr in attrs)
                    {
                        attr.AttachTo(t, prop.Name);
                        lst.Add(attr);
                    }
                }

                return lst;
            });
        }

        protected virtual T ReadWithNoLock<T>(Func<T> func)
        {
            using (GetTransaction())
            {
                return func();
            }
        }


        protected virtual void DoWithNoLock(Action action)
        {
            using (GetTransaction())
            {
                action();
            }
        }
        private static TransactionScope GetTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted });
        }

    }
}
