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
            var now = DateTime.Now;
            // process loading into newItems

            // TODO: use unitOfWorks
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
                           t.DeniedUrlPatterns,
                           Regions = t.AbstractItemAbtractItemRegionArticles.Select(x =>
                               new Region { Id = x.QPRegion_ID, Title = x.QPRegion.Title, Alias = x.QPRegion.Alias })
                       })
                          .ToDictionary(k => k, v => v)
                          .GroupBy(x => x.Value.Discriminator_ID));
            #endregion

            // get discriminators
            // TODO: use services
            var discriminators = ctx.QPDiscriminators.Select(x => x)
                .ToDictionary(k => k.Id, v => v);
            // get regions
            // var regions = ctx.QPRegions.Select(x => new { x.Id, x.Alias })
            //    .ToDictionary(k => k.Id, v => v);

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

                var groupedItems = new Dictionary<int, object>();
                QPDiscriminator discriminator = null;

                if (discriminators.TryGetValue(group.Key.Value, out discriminator))
                {
                    // processing extensions
                    var contentId = discriminator.PreferredContentId;
                    if (contentId != null)
                    {
                        if (!groupedIds.ContainsKey(contentId.Value))
                        {
                            groupedIds.Add(contentId.Value, groupedItems);
                        }
                    }

                    foreach (var item in group)
                    {
                        if ((item.Value.ExtensionId ?? 0) != (contentId ?? 0))
                        {
                            Debug.WriteLine(string.Format("Несоответствие расширения и расширения в PreferredContentId: {0}, {1} for {2} ({3})",
                                item.Value.ExtensionId, contentId, item.Value.Id, item.Value.Name));
                        }


                        var newItem = _activator.CreateInstance(discriminator.Name, false);

                        if (newItem == null)
                        {
                            continue;
                        }

                        ((IInjectable<IUrlParser>)newItem).Set(urlParser);
                        ((IInjectable<ICultureUrlResolver>)newItem).Set(cultureUrlresolver);

                        // TODO: mapping
                        newItem.Regions = new RegionCollection();
                        newItem.Regions.AddRange(item.Value.Regions);
                        if (item.Value.Culture != null)
                            newItem.Culture = new Culture { Id = item.Value.Culture.Id, Key = item.Value.Culture.Name, Name = item.Value.Culture.Name, Title = item.Value.Culture.Title };

                        newItem.Id = item.Value.Id;
                        newItem.Name = item.Value.Name;
                        newItem.Description = item.Value.Description;
                        newItem.Keywords = item.Value.Keywords;
                        newItem.Tags = item.Value.Tags;
                        newItem.ParentId = item.Value.Parent_ID ?? 0;
                        newItem.Title = item.Value.Title;
                        newItem.Name = item.Value.Name;
                        newItem.MetaDescription = item.Value.MetaDescription;
                        newItem.VersionOfId = item.Value.VersionOf_ID;
                        newItem.ExtensionId = item.Value.ExtensionId;
                        newItem.TitleFormat = item.Value.TitleFormatId ?? 0;
                        newItem.ZoneName = item.Value.ZoneName;
                        newItem.IsVisible = item.Value.IsVisible ?? false;
                        newItem.IsInSitemap = item.Value.IsInSiteMap ?? false;
                        newItem.SortOrder = item.Value.IndexOrder ?? 0;
                        newItem.IsPublished = item.Value.StatusTypeName.Equals("published", StringComparison.InvariantCultureIgnoreCase);
                        newItems.Add(newItem.Id, newItem);
                        newItem.Created = item.Value.Created;
                        newItem.Updated = item.Value.Modified;
                        newItem.Loaded = now;

                        if (!string.IsNullOrWhiteSpace(item.Value.AllowedUrlPatterns))
                        {
                            newItem.AllowedUrls = item.Value.AllowedUrlPatterns.SplitString('\n', '\r', ';', ' ', ',');
                        }

                        if (!string.IsNullOrWhiteSpace(item.Value.DeniedUrlPatterns))
                        {
                            newItem.DeniedUrls = item.Value.DeniedUrlPatterns.SplitString('\n', '\r', ';', ' ', ',');
                        }

                        groupedItems.Add(newItem.Id, null);
                    }
                }
            }
            #endregion

            // Ключом служит идентификатор расширения, а не AbstractItem
            var needLoadM2MItems = new Dictionary<int, AbstractItem>();
            // process nodes' extensions
            foreach (var contentIdKey in groupedIds.Keys)
            {
                if (contentIdKey <= 0 || groupedIds[contentIdKey].Keys.Count == 0)
                    continue;

                var def = _manager.GetDefinitions()
                    .FirstOrDefault(x => x.PreferredContentId == contentIdKey);
                if (def == null)
                {
                    Trace.WriteLine(String.Format("Для contentIdKey '{0}' отсутствует ItemDefinition", contentIdKey));
                    continue;
                }

                var results = new DataTable();
                string conStr = LinqHelper.Context.ConnectionString;

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand sqlCmd = new SqlCommand("dbo.qa_extend_items", con);

                    sqlCmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = sqlCmd.Parameters
                        .AddWithValue("@Ids", groupedIds[contentIdKey].Keys.CreateSqlDataRecords().Ensure());

                    // TODO: get current state
                    SqlParameter isLive = sqlCmd.Parameters.AddWithValue("@isLive", !IsStage);
                    SqlParameter contentId = sqlCmd.Parameters.AddWithValue("@contentId", contentIdKey);

                    isLive.SqlDbType = SqlDbType.Bit;
                    contentId.SqlDbType = SqlDbType.Int;
                    tvpParam.SqlDbType = SqlDbType.Structured;

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
                                        stringValue = option.Process(ctx.Cnn, ctx, stringValue);
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

                        if (item.Details.Keys.Contains(column.ColumnName))
                        {
                            throw new InvalidOperationException($"Extension article has incorrect format. The column ${column.ColumnName} already exists. Article id: {item.Id}");
                        }

                        item.Details.Add(column.ColumnName, value is DBNull ? null : value);

                        if (value is decimal
                            && def.NeedLoadM2MRelationsIds
                            && String.Compare(column.ColumnName, "CONTENT_ITEM_ID", true) == 0)
                        {
                            needLoadM2MItems[item.GetDetail<int>("CONTENT_ITEM_ID", 0)] = item;
                        }
                    }
                }
            }

            LoadM2MRelationsIds(needLoadM2MItems);
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

            foreach (var item in newItems.Values)
            {
                if (item.SortOrder == 0 && item.VersionOfId != null && item.VersionOfId > 0)
                {
                    AbstractItem generalItem = null;
                    if (newItems.TryGetValue(item.VersionOfId.Value, out generalItem))
                    {
                        item.SortOrder = generalItem.SortOrder;
                    }
                }

            }

            // process exchanging the data

            lock (locker)
            {
                var items = Model.ItemsInternal;
                AbstractItem root = null;
                items.Clear();
                foreach (var item in newItems)
                {
                    items.Add(item.Key, item.Value);

                    // set the root
                    if (root != null && item.Value.ParentId == null
                        || item.Value.ParentId == item.Value.Id
                        || item.Value.ParentId == 0)
                    {
                        root = item.Value;
                    }
                }
                Model.Root = root;
            }
        }

        protected void LoadM2MRelationsIds(Dictionary<int, AbstractItem> mapItemsToContentItemIDs)
        {
            SqlCommand cmd = new SqlCommand("dbo.qa_extend_items_m2m");
            cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter tvpParam = cmd.Parameters.AddWithValue("@Ids",
                mapItemsToContentItemIDs.Keys.CreateSqlDataRecords()
                .Ensure());

            SqlParameter isLiveParam = cmd.Parameters.AddWithValue("@isLive", !IsStage);
            isLiveParam.SqlDbType = SqlDbType.Bit;
            tvpParam.SqlDbType = SqlDbType.Structured;

            var results = new DataTable();
            using (SqlConnection con = new SqlConnection(LinqHelper.Context.ConnectionString))
            {
                cmd.Connection = con;
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    results.Load(reader);
                }
            }

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
            using (var txn = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                return func();
            }
        }
    }
}
