using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using QA.Core.Engine.Collections;
using QA.Core.Engine.Interface;
using QA.Core.Engine.Web;

namespace QA.Core.Engine
{
    /// <summary>
    /// Базовый класс для всех страниц и виджетов
    /// </summary>
    [DebuggerDisplay("{Name}#{Id}")]
    public class AbstractItem : INode, INameable, IInjectable<IUrlParser>, IInjectable<ICultureUrlResolver>, IPlaceable, IUrlFilterable
    {
        #region Fields

        private DateTime? _published;
        private DateTime _updated;
        private DateTime _created;
        private static char[] invalidCharacters = new char[] { '%', '?', '&', '/', ':' };
        private AbstractItem _parent;
        private string _title;
        private string _name;
        private int _sortOrder;
        private bool _isVisible;
        private bool _isInSiteMap;
        private string _url;
        private string _zoneName;
        private AbstractItem _versionOf;
        private IAbstractItemList<AbstractItem> _children;
        private IAbstractItemList<AbstractItem> _versions;
        private IUrlParser _urlParser;
        private DetailCollection _details;
        private Dictionary<int, List<int>> _allRelationsIds;

        #endregion

        #region Internals

        internal int? ParentId
        {
            get;
            set;
        }

        protected Dictionary<int, List<int>> AllRelationsIds
        {
            get
            {
                if (_allRelationsIds == null)
                    _allRelationsIds = new Dictionary<int, List<int>>();
                return _allRelationsIds;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Список регионов, в которых отображается элемент
        /// </summary>
        public RegionCollection Regions
        {
            get;
            set;
        }
        /// <summary>
        /// Языковая культура, для которой отображается элемент
        /// </summary>
        public Culture Culture
        {
            get;
            set;
        }

        public bool IsPublished
        {
            get;
            set;
        }

        public string CultureToken
        {
            get
            {
                return Culture == null ? null : (Culture.Key ?? "").ToLower();
            }
        }

        [Obsolete]
        public string RegionTokens
        {
            get;
            set;
        }

        /// <summary>
        /// Ид
        /// </summary>
        public virtual int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Признак того, что данный элемент является страницей
        /// </summary>
        public virtual bool IsPage
        {
            get;
            set;
        }

        public string Description { get; set; }
        public string Keywords { get; set; }
        public string Tags { get; set; }

        public virtual AbstractItem Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value != null)
                {
                    ParentId = value.Id;
                    _parent = value;
                }
            }

        }

        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Формат формирования заголовка. null, если формат не задан
        /// </summary>
        public virtual ItemTitleFormat ItemTitleFormat
        {
            get
            {
                return _titleFormat;
            }
            set
            {
                _titleFormat = value;
            }
        }


        public virtual string Name
        {
            get
            {
                return _name ?? (Id > 0 ? Id.ToString() : null);
            }
            set
            {
                //if (value != null && value.IndexOfAny(invalidCharacters) >= 0) throw new Уxception("Invalid characters in name, '%', '?', '&', '/', ':', '+', '.' not allowed.");
                if (string.IsNullOrEmpty(value))
                {
                    _name = null;
                }
                else
                {
                    _name = value;
                }

                _url = null;
            }
        }

        public virtual DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }

        public virtual DateTime Updated
        {
            get { return _updated; }
            set { _updated = value; }
        }

        //public virtual DateTime? Published
        //{
        //    get { return _published; }
        //    set { _published = value; }
        //}

        //public virtual DateTime? Expires
        //{
        //    get { return _expires; }
        //    set { _expires = value != DateTime.MinValue ? value : null; }
        //}

        public virtual int SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
            }
        }

        /// <summary>
        /// Признак отображения в навигации сайта
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
            }
        }

        public virtual bool IsVisibility
        {
            get;
            set;
        }

        /// <summary>
        /// Признак отображения на карте сайта
        /// </summary>
        public virtual bool IsInSitemap
        {
            get
            {
                return _isInSiteMap;
            }
            set
            {
                _isInSiteMap = value;
            }
        }

        /// <summary>
        /// Имя зоны (только для виджетов)
        /// </summary>
        public virtual string ZoneName
        {
            get
            {
                return _zoneName;
            }
            set
            {
                _zoneName = value;
            }
        }

        /// <summary>
        /// Оригинальная страницы (виджет). Применяется только для контентных версий
        /// </summary>
        public virtual AbstractItem VersionOf
        {
            get
            {
                return _versionOf;
            }
            set
            {
                _versionOf = value;
            }
        }

        /// <summary>
        /// Адрес страницы или виджета с учетом региона и языка
        /// </summary>
        public virtual string Url
        {
            get
            {
                // todo: хранить адрес без токенов, а токены подставлять здесь

                string matchedUrl = AncestorTrail;

                if (_cultureUrlResolver != null)
                {
                    matchedUrl = _cultureUrlResolver.AddTokensToUrl(matchedUrl,
                        (Culture == null) ? _cultureUrlResolver.GetCurrentCulture() 
                                : Culture.Key.ToLower(),
                        _cultureUrlResolver.GetCurrentRegion());
                }

                return matchedUrl;
            }
        }

        /// <summary>
        /// Адрес 
        /// </summary>
        public virtual string AncestorTrail
        {
            get
            {
                if (_url == null)
                {
                    if (_urlParser != null)
                        _url = _urlParser.BuildUrl(this);
                    else
                        _url = FindPath(PathData.DefaultAction).RewrittenUrl;
                }

                return _url;
            }
        }

        /// <summary>
        /// Список дочерних элементов
        /// Не учитываются версии
        /// </summary>
        public virtual IAbstractItemList<AbstractItem> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
            }
        }

        /// <summary>
        /// Контентные версии
        /// </summary>
        public virtual IAbstractItemList<AbstractItem> Versions
        {
            get
            {
                return _versions;
            }
            set
            {
                _versions = value;
            }
        }

        /// <summary>
        /// Коллекция дополнительных параметров
        /// </summary>
        public virtual DetailCollection Details
        {
            get
            {
                if (_details == null)
                {
                    _details = new DetailCollection();
                }

                return _details;
            }
        }

        #endregion

        [DebuggerStepThrough]
        public override string ToString()
        {
            return Name + "#" + Id;
        }

        public AbstractItem()
        {
            _created = DateTime.Now;
            _updated = DateTime.Now;
            _published = DateTime.Now;
            _children = new ItemList();
            _versions = new ItemList();
            Regions = new RegionCollection();
        }

        /// <summary>
        /// Поиск  дочернего раздела по url
        /// </summary>
        /// <param name="remainingUrl"></param>
        /// <returns></returns>
        public virtual PathData FindPath(string remainingUrl, string region, string culture)
        {
            if (remainingUrl == null)
                return PathDictionary.GetPath(this, string.Empty);

            remainingUrl = remainingUrl.TrimStart('/');

            if (remainingUrl.Length == 0)
                return PathDictionary.GetPath(this, string.Empty);

            int slashIndex = remainingUrl.IndexOf('/');
            string nameSegment = HttpUtility.UrlDecode(slashIndex < 0 ? remainingUrl : remainingUrl.Substring(0, slashIndex));

            var child = GetChildren(new VersioningFilter(region, culture))
                .FindNamed(nameSegment);

            if (child != null)
            {
                remainingUrl = slashIndex < 0 ? null : remainingUrl.Substring(slashIndex + 1);
                return child.FindPath(remainingUrl, region, culture);
            }

            return PathDictionary.GetPath(this, remainingUrl);
        }

        /// <summary>
        /// Поиск  дочернего раздела по url
        /// </summary>
        /// <param name="remainingUrl"></param>
        /// <returns></returns>
        public virtual PathData FindPath(string remainingUrl)
        {
            return FindPath(remainingUrl, null, null);
        }

        /// <summary>
        /// Получение типа элемента
        /// </summary>
        /// <returns></returns>
        public Type GetContentType()
        {
            return this.GetType();
        }

        #region INode
        /// <summary>
        /// Получение ближайшей страницы
        /// </summary>
        /// <returns></returns>
        public AbstractItem ClosestPage()
        {
            var current = this;
            while (current != null)
            {
                if (current.IsPage)
                    break;

                current = current.Parent;
            }

            return current;
        }

        public virtual string Path
        {
            get
            {
                if (VersionOf != null)
                    return VersionOf.Path;

                string path = "/";
                for (AbstractItem item = this; item.Parent != null && !(item.Parent is IRootPage); item = item.Parent)
                {
                    if (item.Name != null)
                        path = "/" + Uri.EscapeDataString(item.Name) + path;
                    else
                        path = "/" + item.Id + path;
                }
                return path;
            }
        }


        void IInjectable<IUrlParser>.Set(IUrlParser dependency)
        {
            _urlParser = dependency;
        }

        void IInjectable<ICultureUrlResolver>.Set(ICultureUrlResolver dependency)
        {
            _cultureUrlResolver = dependency;
        }

        /// <summary>
        /// Получение оригинального адреса controller/action/... для страницы или виджета
        /// </summary>
        /// <returns></returns>
        public string GetPreviewUrl()
        {
            return FindPath(PathData.DefaultAction).RewrittenUrl;
        }

        public string IconUrl
        {
            get;
            set;
        }

        public string ClassNames
        {
            get;
            set;
        }

        /// <summary>
        /// Признак того, имеет ли данный пользователь доступ к странице
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsAuthorized(System.Security.Principal.IPrincipal user)
        {
            throw new NotImplementedException();
        }

        public string Contents
        {
            get;
            set;

        }

        public string ToolTip
        {
            get;
            set;
        }

        public string Target
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Признак наличия потомков
        /// </summary>
        public bool CheckHasChildren()
        {
            return _children != null && _children.Count > 0;
        }

        /// <summary>
        /// Получение свойств расширения
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">имя</param>
        /// <param name="defaultValue">значение по умолчани</param>
        /// <returns></returns>
        public T GetDetail<T>(string name, T defaultValue)
        {
            if (!Details.ContainsKey(name))
            {
                return defaultValue;
            }
            else
            {
                var value = GetDetail(name, typeof(T));
                if (value == null)
                {
                    return default(T);
                }
                return (T)value;
            }
        }

        /// <summary>
        /// Получение свойств расширения
        /// </summary>
        public object GetDetail(string name, Type type)
        {
            var value = Details[name];
            if (type == typeof(string))
            {
                return Convert.ToString(value);
            }
            else if (value == null)
            {
                return null;
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return Convert.ToDouble(value);
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                return Convert.ToInt32(value);
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                return Convert.ToInt64(value);
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return Convert.ToDateTime(value);
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                return Convert.ToBoolean(value);
            }

            return value;
        }

        /// <summary>
        /// Установка значения дополнительнорму параметру
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetDetail<T>(string name, T value)
        {
            SetDetail(name, typeof(T), value);
        }

        /// <summary>
        /// Установка значения дополнительнорму параметру
        /// </summary>
        public void SetDetail(string name, Type t, object value)
        {
            switch (name)
            {
                case "Title":
                    Title = Convert.ToString(value);
                    break;
                case "Name":
                    Name = Convert.ToString(value);
                    break;
                case "IsPage":
                    IsPage = Convert.ToBoolean(value);
                    break;
                case "IsVisible":
                    IsVisible = Convert.ToBoolean(value);
                    break;
            }

            Details.SetDetail(name, default(int), t, value);
        }

        public void AddRelation(int linkId, int linkedItemId)
        {
            if (!AllRelationsIds.ContainsKey(linkId))
                AllRelationsIds.Add(linkId, new List<int>());
            if (!AllRelationsIds[linkId].Contains(linkedItemId))
                AllRelationsIds[linkId].Add(linkedItemId);
        }

        /// <summary>
        /// Получение списка Id для связи m2m контента-расширения по значению ссылки
        /// </summary>
        /// <param name="linkId">id ссылки</param>
        /// <returns></returns>
        public List<int> GetRelationIds(int linkId)
        {
            if (linkId == default(int))
                return new List<int>();

            return AllRelationsIds.ContainsKey(linkId) ? AllRelationsIds[linkId] : new List<int>();
        }

        /// <summary>
        /// Получение списка Id для связи m2m контента-расширения по имени поля
        /// </summary>
        /// <param name="name">имя поля</param>
        public List<int> GetRelationIds(String name)
        {
            return GetRelationIds(GetDetail<int>(name, 0));
        }

        /// <summary>
        /// Получить список дочерних разделов.
        /// Учитываются контентные версии.
        /// </summary>
        /// <returns></returns>
        public ItemList GetChildren()
        {
            //// search for first structural page
            //// and get its' children
            AbstractItem initialItem = VersionOf ?? this;

            return new ItemList(initialItem.Children, new AccessFilter());
        }

        public ItemList GetChildren(ItemFilter filter)
        {
            AbstractItem initialItem = VersionOf ?? this;

            return new ItemList(initialItem.Children, filter);
        }

        public virtual AbstractItem GetChild(string childName)
        {
            if (string.IsNullOrEmpty(childName))
                return null;

            int slashIndex = childName.IndexOf('/');
            if (slashIndex == 0) // starts with slash
            {
                if (childName.Length == 1)
                    return this;
                else
                    return GetChild(childName.Substring(1));
            }
            if (slashIndex > 0) // contains a slash
            {
                string nameSegment = HttpUtility.UrlDecode(childName.Substring(0, slashIndex));
                foreach (AbstractItem child in GetChildren(new NullFilter()))
                {
                    if (child.IsNamed(nameSegment))
                    {
                        return child.GetChild(childName.Substring(slashIndex));
                    }
                }
                return null;
            }

            // no slash, only a name
            foreach (AbstractItem child in GetChildren(new NullFilter()))
            {
                if (child.IsNamed(childName))
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// проверка соответствия алиаса элементу
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual bool IsNamed(string name)
        {
            if (Name == null)
                return false;
            return Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                || (Name + Extension).Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        private string _extension = "";
        private ICultureUrlResolver _cultureUrlResolver;
        private ItemTitleFormat _titleFormat;

        /// <summary>
        /// Расширение страницы (например, .aspx)
        /// </summary>
        public string Extension
        {
            get
            {
                return _extension;
            }
            set
            {
                _extension = value;
            }
        }

        /// <summary>
        /// Статус элемента
        /// </summary>
        public ItemState State
        {
            get;
            set;
        }

        /// <summary>
        /// Идентификатор оригинальной страницы
        /// </summary>
        public int? VersionOfId
        {
            get;
            set;
        }

        /// <summary>
        /// Признак наличия потомков
        /// </summary>
        public bool HasChildren
        {
            get;
            set;
        }

        /// <summary>
        /// Признак наличия контентных версий
        /// </summary>
        public bool HasContentVersions
        {
            get;
            set;
        }

        /// <summary>
        /// Идентификатор типа
        /// </summary>
        public int? DiscriminatorId
        {
            get;
            set;
        }

        /// <summary>
        /// Название типа
        /// </summary>
        public string DiscriminatorName
        {
            get;
            set;
        }

        /// <summary>
        /// Описание типа
        /// </summary>
        public string DiscriminatorTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Расширение
        /// </summary>
        public int? ExtensionId
        {
            get;
            set;
        }

        public virtual int TitleFormat { get; set; }

        public virtual string MetaDescription { get; set; }

        #region IUrlFilterable Members

        public IEnumerable<string> AllowedUrls
        {
            get;
            set;
        }

        public IEnumerable<string> DeniedUrls
        {
            get;
            set;
        }

        #endregion

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AbstractItem))
            {
                return false;
            }

            return (obj as AbstractItem).Id == Id;
        }
    }
}
