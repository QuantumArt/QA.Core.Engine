
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using System;
using System.Collections;
using System.Collections.Generic;

using System.ComponentModel;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using HttpContext = System.Web.HttpContext;

#pragma warning disable 1591



namespace QA.Core.Engine.QPData
{

    public interface IQPContent
    {
        int Id { get; set; }
        int StatusTypeId { get; set; }
        StatusType StatusType { get; set; }
        bool StatusTypeChanged { get; set; }
        bool Visible { get; set; }
        bool Archive { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
        int LastModifiedBy { get; set; }
    }

    public interface IQPLink
    {
        int Id { get; }
        int LinkedItemId { get; }
        int LinkId { get; }
        bool InsertWithArticle { get; set; }
        bool RemoveWithArticle { get; set; }
        void SaveInsertingInstruction();
        void SaveRemovingInstruction();
        string InsertingInstruction { get; }
        string RemovingInstruction { get; }
    }

    public abstract class QPEntityBase
    {
        protected QPContext _InternalDataContext = LinqHelper.Context;

        public QPContext InternalDataContext
        {
            get { return _InternalDataContext; }
            set { _InternalDataContext = value; }
        }

        public abstract void Detach();

        protected static EntityRef<T> Detach<T>(EntityRef<T> entity) where T : QPEntityBase
        {
            if (!entity.HasLoadedOrAssignedValue || entity.Entity == null)
                return new EntityRef<T>();
            entity.Entity.Detach();
            return new EntityRef<T>(entity.Entity);
        }

        protected static EntitySet<T> Detach<T>(EntitySet<T> set, Action<T> onAdd, Action<T> onRemove) where T : QPEntityBase
        {
            if (set == null || !set.HasLoadedOrAssignedValues)
                return new EntitySet<T>(onAdd, onRemove);

            var list = set.ToList();
            list.ForEach(t => t.Detach());

            var newSet = new EntitySet<T>(onAdd, onRemove);
            newSet.Assign(list);
            return newSet;
        }

        protected static Link<T> Detach<T>(Link<T> value)
        {
            if (!value.HasLoadedOrAssignedValue)
                return default(Link<T>);

            return new Link<T>(value.Value);
        }
    }

    public abstract class QPLinkBase : QPEntityBase
    {

        private bool _insertWithArticle = false;
        public bool InsertWithArticle
        {
            get
            {
                return _insertWithArticle;
            }
            set
            {
                _insertWithArticle = value;
            }
        }

        private bool _removeWithArticle = false;
        public bool RemoveWithArticle
        {
            get
            {
                return _removeWithArticle;
            }
            set
            {
                _removeWithArticle = value;
            }
        }

        private string _insertingInstruction;
        public string InsertingInstruction
        {
            get
            {
                if (String.IsNullOrEmpty(_insertingInstruction))
                    SaveInsertingInstruction();
                return _insertingInstruction;
            }
        }


        private string _removingInstruction;
        public string RemovingInstruction
        {
            get
            {
                if (String.IsNullOrEmpty(_removingInstruction))
                    SaveRemovingInstruction();
                return _removingInstruction;
            }
        }

        public void SaveRemovingInstruction()
        {
            _removingInstruction = String.Format("EXEC sp_executesql N'EXEC qp_delete_single_link @linkId, @itemId, @linkedItemId', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", this.LinkId, this.Id, this.LinkedItemId);
        }

        public void SaveInsertingInstruction()
        {
            _insertingInstruction = String.Format("EXEC sp_executesql N'EXEC qp_insert_single_link @linkId, @itemId, @linkedItemId', N'@linkId NUMERIC, @itemId NUMERIC, @linkedItemId NUMERIC', @linkId = {0}, @itemId = {1}, @linkedItemId = {2}", this.LinkId, this.Id, this.LinkedItemId);
        }

        public abstract int LinkId { get; }
        public abstract int Id { get; }
        public abstract int LinkedItemId { get; }


    }

    public abstract class QPContentBase : QPEntityBase
    {
        protected bool _StatusTypeChanged;
        public bool StatusTypeChanged
        {
            get { return _StatusTypeChanged; }
            set { _StatusTypeChanged = value; }
        }

        protected void HandlePropertyChangedEvent(object sender, PropertyChangedEventArgs a)
        {
            if (a.PropertyName == "StatusType")
            {
                _StatusTypeChanged = true;
            }
        }
    }

    public partial class StatusType: QPEntityBase
    {
        public override void Detach()
        {
                if (null == PropertyChanging)
                    return;

                PropertyChanging = null;
                PropertyChanged = null;
        }
    }

    internal static partial class LinqHelper
{

    private static string _Key = Guid.NewGuid().ToString();
    private static string Key
    {
        get
        {
            return "LinqUtilDataContextKey " + _Key;
        }
    }

    [ThreadStatic]
    private	static QPContext _context;
    private	static QPContext InternalDataContext
    {
        get
        {
            if (HttpContext.Current	== null)
                return _context;
            else
                return (QPContext)HttpContext.Current.Items[Key];
            }
        set
        {
            if (HttpContext.Current	== null)
                _context = value;
            else
                HttpContext.Current.Items[Key] = value;
        }
    }

    public static QPContext Context
    {
        get
        {
            if (InternalDataContext	== null)
            {
                InternalDataContext	=	QPContext.Create();
            }
            return InternalDataContext;
        }
    }
}



public partial class QPContext
{


    private static string _DefaultSiteName = "main_site";

    public static string DefaultSiteName {
        get
        {
            return _DefaultSiteName;
        }
        set
        {
            _DefaultSiteName = value;
        }
    }

    private static string _DefaultConnectionString;

    public static string DefaultConnectionString {
        get
        {
            if (_DefaultConnectionString == null)
            {
                var obj = System.Configuration.ConfigurationManager.ConnectionStrings["qp_database"];
                if (obj == null)
                    throw new ApplicationException("Connection string 'qp_database' is not specified");
                _DefaultConnectionString = obj.ConnectionString;
            }
            return _DefaultConnectionString;
        }
        set
        {
            _DefaultConnectionString = value;
        }
    }

    private static XmlMappingSource _DefaultXmlMappingSource;

    public static XmlMappingSource DefaultXmlMappingSource {
        get
        {
            if (_DefaultXmlMappingSource == null)
            {
                _DefaultXmlMappingSource = GetDefaultXmlMappingSource(null);
            }
            return _DefaultXmlMappingSource;
        }
        set
        {
            _DefaultXmlMappingSource = value;
        }
    }

    public static XmlMappingSource GetDefaultXmlMappingSource(IDbConnection connection)
    {
        DBConnector dbc = (connection != null) ? new DBConnector(connection) : new DBConnector(DefaultConnectionString);
        return XmlMappingSource.FromXml(dbc.GetDefaultMapFileContents(dbc.GetSiteId(DefaultSiteName)));
    }

    public static QPContext Create(IDbConnection connection, string siteName, MappingSource mappingSource)
    {
        QPContext ctx = new QPContext(connection, mappingSource);
        ctx.SiteName = siteName;
        ctx.ConnectionString = connection.ConnectionString;
        return ctx;
    }

    public static QPContext Create(IDbConnection connection, string siteName)
    {
        return Create(connection, siteName, GetDefaultXmlMappingSource(connection));
    }

    public static QPContext Create(IDbConnection connection)
    {
        return Create(connection, DefaultSiteName);
    }

    public static QPContext Create(string connection, string siteName, MappingSource mappingSource)
    {
        QPContext ctx = new QPContext(connection, mappingSource);
        ctx.SiteName = siteName;
        ctx.ConnectionString = connection;
        return ctx;
    }

    public static QPContext Create(string siteName, MappingSource mappingSource)
    {
        return Create(DefaultConnectionString, siteName, mappingSource);
    }

    public static QPContext Create(string connection, string siteName)
    {
        return Create(connection, siteName, DefaultXmlMappingSource);
    }

    public static QPContext Create(string connection)
    {
        return Create(connection, DefaultSiteName);
    }

    public static QPContext Create()
    {
        return Create(DefaultConnectionString);
    }

    public string ConnectionString { get; private set; }

    private string _SiteName;
    public string SiteName
    {
        get
        {
            return _SiteName;
        }
        set
        {
            if (!String.Equals(_SiteName, value, StringComparison.InvariantCultureIgnoreCase))
            {
                _SiteName = value;
                SiteId = Cnn.GetSiteId(_SiteName);
                LoadSiteSpecificInfo();
            }
        }
    }

    public Int32 SiteId { get; private set; }



    private DBConnector _cnn;
    public DBConnector Cnn
    {
        get
        {
            if (_cnn == null)
            {
                _cnn = (Connection != null) ? new DBConnector(Connection, Transaction, new DbConnectorSettings(), new MemoryCache(new MemoryCacheOptions()), null) : new DBConnector(ConnectionString);
                _cnn.UpdateManyToOne = false;
                _cnn.ThrowNotificationExceptions = false;
            }
            return _cnn;
        }
    }

    public static bool RemoveUploadUrlSchema = false;
    private bool _ShouldRemoveSchema = false;

  public bool ShouldRemoveSchema { get { return _ShouldRemoveSchema; } set { _ShouldRemoveSchema = value; }}

    public void LoadSiteSpecificInfo()
    {
        if (RemoveUploadUrlSchema && !_ShouldRemoveSchema)
        {
                _ShouldRemoveSchema = RemoveUploadUrlSchema;
        }

        LiveSiteUrl = Cnn.GetSiteUrl(SiteId, true);
        LiveSiteUrlRel = Cnn.GetSiteUrlRel(SiteId, true);
        StageSiteUrl = Cnn.GetSiteUrl(SiteId, false);
        StageSiteUrlRel = Cnn.GetSiteUrlRel(SiteId, false);
        LongUploadUrl = Cnn.GetImagesUploadUrl(SiteId, false, _ShouldRemoveSchema);
        ShortUploadUrl = Cnn.GetImagesUploadUrl(SiteId, true, _ShouldRemoveSchema);
        PublishedId = Cnn.GetMaximumWeightStatusTypeId(SiteId);
    }

    public string SiteUrl
    {
        get
        {
            return (Cnn.IsStage) ? StageSiteUrl : LiveSiteUrl;
        }
    }

    public string UploadUrl
    {
        get
        {
            return LongUploadUrl;
        }
    }

    public string LiveSiteUrl { get; private set; }

    public string LiveSiteUrlRel { get; private set; }

    public string StageSiteUrl { get; private set; }

    public string StageSiteUrlRel { get; private set; }

    public string LongUploadUrl { get; private set; }

    public string ShortUploadUrl { get; private set; }

    public Int32 PublishedId { get; private set; }


    private string uploadPlaceholder = "<%=upload_url%>";
    private string sitePlaceholder = "<%=site_url%>";

    public string ReplacePlaceholders(string input)
    {
        string result = input;
        if (result != null)
        {
            result = result.Replace(uploadPlaceholder, UploadUrl);
            result = result.Replace(sitePlaceholder, SiteUrl);
        }
        return result;
    }

    public override void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
    {
        Cnn.ExternalTransaction = Transaction;
        ChangeSet delta = GetChangeSet();

        foreach (var delete in delta.Deletes.OfType<IQPLink>().Where(n => !n.RemoveWithArticle))
        {
            delete.SaveRemovingInstruction();
        }

        foreach (var insert in delta.Inserts.OfType<IQPLink>().Where(n => !n.InsertWithArticle))
        {
            insert.SaveInsertingInstruction();
        }



        base.SubmitChanges(failureMode);

        foreach (var delete in delta.Deletes.OfType<IQPLink>().Where(n => !n.RemoveWithArticle))
        {
            Cnn.ProcessData(delete.RemovingInstruction);
        }

        foreach (var insert in delta.Inserts.OfType<IQPLink>().Where(n => !n.InsertWithArticle))
        {
            Cnn.ProcessData(insert.InsertingInstruction);
        }


    }

    protected Dictionary<string, string> GetInitialValues(IQPContent instance)
    {
        return new Dictionary<string, string>
        {
            [SystemColumnNames.Id] = instance.Id.ToString(),
            [SystemColumnNames.Visible] = instance.Visible ? "1" : "0",
            [SystemColumnNames.Archive] = instance.Archive ? "1" : "0",
            [SystemColumnNames.StatusTypeId] = instance.StatusTypeId.ToString()
        };
    }
}


public static class UserExtensions
{


    public static IEnumerable<QPAbstractItem> ApplyContext(this IEnumerable<QPAbstractItem> e, QPContext context)
    {
        foreach(QPAbstractItem item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPAbstractItem> Published(this IQueryable<QPAbstractItem> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPAbstractItem> ForFrontEnd(this IQueryable<QPAbstractItem> e)
    {
        return e;
    }


    public static IEnumerable<QPDiscriminator> ApplyContext(this IEnumerable<QPDiscriminator> e, QPContext context)
    {
        foreach(QPDiscriminator item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPDiscriminator> Published(this IQueryable<QPDiscriminator> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPDiscriminator> ForFrontEnd(this IQueryable<QPDiscriminator> e)
    {
        return e;
    }


    public static IEnumerable<QPCulture> ApplyContext(this IEnumerable<QPCulture> e, QPContext context)
    {
        foreach(QPCulture item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPCulture> Published(this IQueryable<QPCulture> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPCulture> ForFrontEnd(this IQueryable<QPCulture> e)
    {
        return e;
    }


    public static IEnumerable<ItemTitleFormat> ApplyContext(this IEnumerable<ItemTitleFormat> e, QPContext context)
    {
        foreach(ItemTitleFormat item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<ItemTitleFormat> Published(this IQueryable<ItemTitleFormat> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<ItemTitleFormat> ForFrontEnd(this IQueryable<ItemTitleFormat> e)
    {
        return e;
    }


    public static IEnumerable<QPRegion> ApplyContext(this IEnumerable<QPRegion> e, QPContext context)
    {
        foreach(QPRegion item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPRegion> Published(this IQueryable<QPRegion> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPRegion> ForFrontEnd(this IQueryable<QPRegion> e)
    {
        return e;
    }


    public static IEnumerable<TrailedAbstractItem> ApplyContext(this IEnumerable<TrailedAbstractItem> e, QPContext context)
    {
        foreach(TrailedAbstractItem item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<TrailedAbstractItem> Published(this IQueryable<TrailedAbstractItem> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<TrailedAbstractItem> ForFrontEnd(this IQueryable<TrailedAbstractItem> e)
    {
        return e;
    }


    public static IEnumerable<QPObsoleteUrl> ApplyContext(this IEnumerable<QPObsoleteUrl> e, QPContext context)
    {
        foreach(QPObsoleteUrl item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPObsoleteUrl> Published(this IQueryable<QPObsoleteUrl> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPObsoleteUrl> ForFrontEnd(this IQueryable<QPObsoleteUrl> e)
    {
        return e;
    }


    public static IEnumerable<QPItemDefinitionConstraint> ApplyContext(this IEnumerable<QPItemDefinitionConstraint> e, QPContext context)
    {
        foreach(QPItemDefinitionConstraint item in e)
            item.InternalDataContext = context;
        return e;
    }

    public static IQueryable<QPItemDefinitionConstraint> Published(this IQueryable<QPItemDefinitionConstraint> e)
    {
        return e.Where(n => n.StatusType.Name == "Published");
    }

    public static IQueryable<QPItemDefinitionConstraint> ForFrontEnd(this IQueryable<QPItemDefinitionConstraint> e)
    {
        return e;
    }

}


public class BindingListSelector<TSource, T> : ListSelector<TSource, T>, IBindingList
{
    public BindingListSelector(IBindingList source, Func<TSource, T> selector, Action<IList<TSource>, T> onAdd, Action<IList<TSource>, T> onRemove):base(source as IList<TSource>, selector, onAdd, onRemove)
    {
        sourceAsBindingList = source;
    }

    protected IBindingList sourceAsBindingList;

    #region IBindingList Members

    public void AddIndex(PropertyDescriptor property)
    {
        sourceAsBindingList.AddIndex(property);
    }

    public object AddNew()
    {
        return sourceAsBindingList.AddNew();
    }

    public bool AllowEdit
    {
        get { return sourceAsBindingList.AllowEdit; }
    }

    public bool AllowNew
    {
        get { return sourceAsBindingList.AllowNew; }
    }

    public bool AllowRemove
    {
        get { return sourceAsBindingList.AllowRemove; }
    }

    public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
    {
        sourceAsBindingList.ApplySort(property, direction);
    }

    public int Find(PropertyDescriptor property, object key)
    {
        return sourceAsBindingList.Find(property, key);
    }

    public bool IsSorted
    {
        get { return sourceAsBindingList.IsSorted; }
    }

    public event ListChangedEventHandler ListChanged
    {
        add
        {
            sourceAsBindingList.ListChanged += value;
        }
        remove
        {
            sourceAsBindingList.ListChanged -= value;
        }
    }

    public void RemoveIndex(PropertyDescriptor property)
    {
        sourceAsBindingList.RemoveIndex(property);
    }

    public void RemoveSort()
    {
        sourceAsBindingList.RemoveSort();
    }

    public ListSortDirection SortDirection
    {
        get { return sourceAsBindingList.SortDirection; }
    }

    public PropertyDescriptor SortProperty
    {
        get { return sourceAsBindingList.SortProperty; }
    }

    public bool SupportsChangeNotification
    {
        get { return sourceAsBindingList.SupportsChangeNotification; }
    }

    public bool SupportsSearching
    {
        get { return sourceAsBindingList.SupportsSearching; }
    }

    public bool SupportsSorting
    {
        get { return sourceAsBindingList.SupportsSorting; }
    }

    #endregion
}

public static class ListSelectorExtensions
{
    public static ListSelector<TSource, T> AsListSelector<TSource, T>(this IList<TSource> source, Func<TSource, T> selector, Action<IList<TSource>, T> onAdd, Action<IList<TSource>, T> onRemove)
    {
        return new ListSelector<TSource, T>(source, selector, onAdd, onRemove);
    }
    public static BindingListSelector<TSource, T> AsListSelector<TSource, T>(this IBindingList source, Func<TSource, T> selector, Action<IList<TSource>, T> onAdd, Action<IList<TSource>, T> onRemove)
    {
        return new BindingListSelector<TSource, T>(source, selector, onAdd, onRemove);
    }
}

public class ListSelector<TSource, T> : IList<T>, IList
{
    public ListSelector(IList<TSource> source, Func<TSource, T> selector, Action<IList<TSource>, T> onAdd, Action<IList<TSource>, T> onRemove)
    {
        this.source = source;
        this.selector = selector;
        this.onAdd = onAdd;
        this.onRemove = onRemove;
        UpdateProjection();
    }

    protected IList<TSource> source;
    protected Func<TSource, T> selector;
    protected List<T> projection;
    protected Action<IList<TSource>, T> onAdd;
    protected Action<IList<TSource>, T> onRemove;

    #region IList<T> Members

    public int IndexOf(T item)
    {
        int i = 0;
        foreach (T t in projection)
        {
            if (t.Equals(item))
            return i;
            i++;
        }
        return -1;
    }

    private void UpdateProjection()
    {
        projection = source.Select(selector).Where(n => n != null).ToList();
    }

    public void Insert(int index, T item)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public void RemoveAt(int index)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public T this[int index]
    {
        get
        {
            return projection[index];
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    #endregion

    #region ICollection<T> Members

    public void Add(T item)
    {
        if (onAdd != null) {
            onAdd(source, item);
            UpdateProjection();
        }
    }

    public void Add(IEnumerable<T> items)
    {
        if (items != null) {
            foreach (T item in items)
            {
                Add(item);
            }
        }
    }

    public void Remove(IEnumerable<T> items)
    {
        if (items != null) {
            foreach (T item in items.ToList())
            {
                Remove(item);
            }
        }
    }

    public bool Remove(T item)
    {
        if (onRemove != null)
        {
            onRemove(source, item);
            UpdateProjection();
            return true;
        }
        else
            return false;
    }

    public void Clear()
    {
        foreach (T item in projection.ToList())
        {
            Remove(item);
        }
    }

    public bool Contains(T item)
    {
        return projection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        projection.CopyTo(array, arrayIndex);
    }

    public int Count
    {
        get { return projection.Count(); }
    }

    public bool IsReadOnly
    {
        get { return true; }
    }

    #endregion

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
        return projection.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return projection.GetEnumerator();
    }

    #endregion

    #region IList Members

    int IList.Add(object value)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    void IList.Clear()
    {
        throw new Exception("The method or operation is not implemented.");
    }

    bool IList.Contains(object value)
    {
        return Contains((T) value);
    }

    int IList.IndexOf(object value)
    {
        return IndexOf((T) value);
    }

    void IList.Insert(int index, object value)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    bool IList.IsFixedSize
    {
        get { return true; }
    }

    bool IList.IsReadOnly
    {
        get { return true; }
    }

    void IList.Remove(object value)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    void IList.RemoveAt(int index)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    object IList.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {

        }
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo(Array array, int index)
    {
        T[] arrayOfT = array as T[];
        if (arrayOfT == null)
            throw new ArgumentException("Incorrect array type");
        this.CopyTo(arrayOfT, index);
    }

    int ICollection.Count
    {
        get { return this.Count; }
    }

    bool ICollection.IsSynchronized
    {
        get { return false; }
    }

    object ICollection.SyncRoot
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    #endregion
}

    partial class QPAbstractItem : QPContentBase, IQPContent
    {

        public QPAbstractItem(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._Parent1 = Detach(this._Parent1);

            this._Discriminator1 = Detach(this._Discriminator1);

            this._VersionOf1 = Detach(this._VersionOf1);

            this._Culture1 = Detach(this._Culture1);

            this._TitleFormat1 = Detach(this._TitleFormat1);

            this._AbstractItemAbtractItemRegionArticles = Detach(this._AbstractItemAbtractItemRegionArticles, attach_AbstractItemAbtractItemRegionArticles, detach_AbstractItemAbtractItemRegionArticles);

            this._Children = Detach(this._Children, attach_Children, detach_Children);

            this._Versions = Detach(this._Versions, attach_Versions, detach_Versions);

            this._ObsoleteUrls = Detach(this._ObsoleteUrls, attach_ObsoleteUrls, detach_ObsoleteUrls);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Title = InternalDataContext.ReplacePlaceholders(_Title);
            _Name = InternalDataContext.ReplacePlaceholders(_Name);
            _ZoneName = InternalDataContext.ReplacePlaceholders(_ZoneName);
            _AllowedUrlPatterns = InternalDataContext.ReplacePlaceholders(_AllowedUrlPatterns);
            _DeniedUrlPatterns = InternalDataContext.ReplacePlaceholders(_DeniedUrlPatterns);
            _Description = InternalDataContext.ReplacePlaceholders(_Description);
            _Keywords = InternalDataContext.ReplacePlaceholders(_Keywords);
            _MetaDescription = InternalDataContext.ReplacePlaceholders(_MetaDescription);
            _Tags = InternalDataContext.ReplacePlaceholders(_Tags);
            _AuthenticationTargeting = InternalDataContext.ReplacePlaceholders(_AuthenticationTargeting);
            _Targeting = InternalDataContext.ReplacePlaceholders(_Targeting);
        }


        public System.Boolean IsVisibleExact
        {
            get
            {
                return (IsVisible.HasValue) ? IsVisible.Value : default(System.Boolean);
            }
        }

        public System.Boolean IsPageExact
        {
            get
            {
                return (IsPage.HasValue) ? IsPage.Value : default(System.Boolean);
            }
        }

        public System.Int32 ContentIdExact
        {
            get
            {
                return (ContentId.HasValue) ? ContentId.Value : default(System.Int32);
            }
        }

        public System.Boolean IsInSiteMapExact
        {
            get
            {
                return (IsInSiteMap.HasValue) ? IsInSiteMap.Value : default(System.Boolean);
            }
        }

        public System.Int32 IndexOrderExact
        {
            get
            {
                return (IndexOrder.HasValue) ? IndexOrder.Value : default(System.Int32);
            }
        }

        public System.Int32 ExtensionIdExact
        {
            get
            {
                return (ExtensionId.HasValue) ? ExtensionId.Value : default(System.Int32);
            }
        }

    }


    partial class QPDiscriminator : QPContentBase, IQPContent
    {

        public QPDiscriminator(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._AllowedItemDefinitions = Detach(this._AllowedItemDefinitions, attach_AllowedItemDefinitions, detach_AllowedItemDefinitions);

            this._ItemDefinitionItemDefinitionArticles = Detach(this._ItemDefinitionItemDefinitionArticles, attach_ItemDefinitionItemDefinitionArticles, detach_ItemDefinitionItemDefinitionArticles);

            this._Items = Detach(this._Items, attach_Items, detach_Items);

            this._AllowDefinition = Detach(this._AllowDefinition, attach_AllowDefinition, detach_AllowDefinition);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Title = InternalDataContext.ReplacePlaceholders(_Title);
            _Name = InternalDataContext.ReplacePlaceholders(_Name);
            _CategoryName = InternalDataContext.ReplacePlaceholders(_CategoryName);
            _Description = InternalDataContext.ReplacePlaceholders(_Description);
            _AllowedZones = InternalDataContext.ReplacePlaceholders(_AllowedZones);
        }



        private string _IconUrlUrl;

        public string IconUrlUrl
        {
            get
            {
                if (String.IsNullOrEmpty(IconUrl))
                    return String.Empty;
                else
                {
                    if (_IconUrlUrl == null)
                        _IconUrlUrl = String.Format(@"{0}/{1}", InternalDataContext.Cnn.GetUrlForFileAttribute(InternalDataContext.Cnn.GetAttributeIdByNetNames(InternalDataContext.SiteId, "QPDiscriminator", "IconUrl"), true, _InternalDataContext.ShouldRemoveSchema), IconUrl);
                    return _IconUrlUrl;
                }
            }
        }

        private string _IconUrlUploadPath;

        public string IconUrlUploadPath
        {
            get
            {
                if (_IconUrlUploadPath == null)
                    _IconUrlUploadPath = InternalDataContext.Cnn.GetDirectoryForFileAttribute(InternalDataContext.Cnn.GetAttributeIdByNetNames(InternalDataContext.SiteId, "QPDiscriminator", "IconUrl"));

                return (_IconUrlUploadPath);
            }
        }


        public System.Int32 PreferredContentIdExact
        {
            get
            {
                return (PreferredContentId.HasValue) ? PreferredContentId.Value : default(System.Int32);
            }
        }

        public System.Boolean IsPageExact
        {
            get
            {
                return (IsPage.HasValue) ? IsPage.Value : default(System.Boolean);
            }
        }

        public System.Boolean FilterPartByUrlExact
        {
            get
            {
                return (FilterPartByUrl.HasValue) ? FilterPartByUrl.Value : default(System.Boolean);
            }
        }

    }


    partial class QPCulture : QPContentBase, IQPContent
    {

        public QPCulture(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._AbstractItems = Detach(this._AbstractItems, attach_AbstractItems, detach_AbstractItems);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Title = InternalDataContext.ReplacePlaceholders(_Title);
            _Name = InternalDataContext.ReplacePlaceholders(_Name);
            _ChangeCultureLabel = InternalDataContext.ReplacePlaceholders(_ChangeCultureLabel);
            _CultureSelectionTitle = InternalDataContext.ReplacePlaceholders(_CultureSelectionTitle);
        }



        private string _IconUrl;

        public string IconUrl
        {
            get
            {
                if (String.IsNullOrEmpty(Icon))
                    return String.Empty;
                else
                {
                    if (_IconUrl == null)
                        _IconUrl = String.Format(@"{0}/{1}", InternalDataContext.Cnn.GetUrlForFileAttribute(InternalDataContext.Cnn.GetAttributeIdByNetNames(InternalDataContext.SiteId, "QPCulture", "Icon"), true, _InternalDataContext.ShouldRemoveSchema), Icon);
                    return _IconUrl;
                }
            }
        }

        private string _IconUploadPath;

        public string IconUploadPath
        {
            get
            {
                if (_IconUploadPath == null)
                    _IconUploadPath = InternalDataContext.Cnn.GetDirectoryForFileAttribute(InternalDataContext.Cnn.GetAttributeIdByNetNames(InternalDataContext.SiteId, "QPCulture", "Icon"));

                return (_IconUploadPath);
            }
        }


        public System.Boolean EnabledExact
        {
            get
            {
                return (Enabled.HasValue) ? Enabled.Value : default(System.Boolean);
            }
        }

        public System.Int32 SortOrderExact
        {
            get
            {
                return (SortOrder.HasValue) ? SortOrder.Value : default(System.Int32);
            }
        }

    }


    partial class ItemTitleFormat : QPContentBase, IQPContent
    {

        public ItemTitleFormat(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._Item = Detach(this._Item, attach_Item, detach_Item);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Value = InternalDataContext.ReplacePlaceholders(_Value);
            _Description = InternalDataContext.ReplacePlaceholders(_Description);
        }


    }


    partial class QPRegion : QPContentBase, IQPContent
    {

        public QPRegion(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Title = InternalDataContext.ReplacePlaceholders(_Title);
            _Alias = InternalDataContext.ReplacePlaceholders(_Alias);
        }


        public System.Int32 ParentIdExact
        {
            get
            {
                return (ParentId.HasValue) ? ParentId.Value : default(System.Int32);
            }
        }

    }


    partial class TrailedAbstractItem : QPContentBase, IQPContent
    {

        public TrailedAbstractItem(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Trail = InternalDataContext.ReplacePlaceholders(_Trail);
            _Title = InternalDataContext.ReplacePlaceholders(_Title);
            _Name = InternalDataContext.ReplacePlaceholders(_Name);
        }


    }


    partial class QPObsoleteUrl : QPContentBase, IQPContent
    {

        public QPObsoleteUrl(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._AbstractItem1 = Detach(this._AbstractItem1);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Url = InternalDataContext.ReplacePlaceholders(_Url);
        }


    }


    partial class QPItemDefinitionConstraint : QPContentBase, IQPContent
    {

        public QPItemDefinitionConstraint(QPContext context)
        {
            _InternalDataContext = context;
            OnCreated();
        }

        public void LoadStatusType()
        {
            _StatusType.Entity = InternalDataContext.StatusTypes.Single(n => n.Id == _StatusTypeId);
        }

        public override void Detach()
        {
            if (null == PropertyChanging)
                return;

            PropertyChanging = null;
            PropertyChanged = null;


            this._Source1 = Detach(this._Source1);

            this._Target1 = Detach(this._Target1);

            this._StatusType = Detach(this._StatusType);
        }

        partial void OnCreated()
        {
            if (_InternalDataContext == null)
                _InternalDataContext = LinqHelper.Context;
            _Visible = true;
            _Archive = false;
            _StatusTypeId = _InternalDataContext.PublishedId;
            _StatusTypeChanged = false;
            PropertyChanged += HandlePropertyChangedEvent;
        }

        partial void OnLoaded()
        {

            _Title = InternalDataContext.ReplacePlaceholders(_Title);
        }


    }


}
