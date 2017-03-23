using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace QA.Core.Engine
{
    /// <summary>
    /// Класс для работы с адресом
    /// </summary>
    public class Url
    {
        #region Fields
        const string Amp = "&";
        const string ManagementUrlToken = "{ManagementUrl}";

        static readonly string[] querySplitter = new[] { "&amp;", Amp };
        static readonly char[] slashes = new char[] { '/' };
        static readonly char[] dotsAndSlashes = new char[] { '.', '/' };
        static string defaultExtension = "";
        static string defaultDocument = "Default.aspx";

        static Dictionary<string, string> replacements = new Dictionary<string, string> { { ManagementUrlToken, "~/cms/managment" } };

        string _scheme;
        string _authority;
        string _path;
        string _query;
        string _fragment;
        bool? _forcedAddTrailingSlashes = null;
        #endregion

        public Url(Url other)
        {
            _scheme = other._scheme;
            _authority = other._authority;
            _path = other._path;
            _query = other._query;
            _fragment = other._fragment;
            _forcedAddTrailingSlashes = other._forcedAddTrailingSlashes;
        }

        public Url(string scheme, string authority, string path, string query, string fragment, bool? addTrailingSlash = null)
        {
            _forcedAddTrailingSlashes = addTrailingSlash;
            _scheme = scheme;
            _authority = authority;
            _path = CheckPath(path);
            _query = query;
            _fragment = fragment;
        }

        public Url(string scheme, string authority, string rawUrl, bool? addTrailingSlash = null)
        {
            _forcedAddTrailingSlashes = addTrailingSlash;
            int queryIndex = QueryIndex(rawUrl);
            int hashIndex = rawUrl.IndexOf('#', queryIndex > 0 ? queryIndex : 0);
            LoadFragment(rawUrl, hashIndex);
            LoadQuery(rawUrl, queryIndex, hashIndex);
            LoadSiteRelativeUrl(rawUrl, queryIndex, hashIndex);
            _scheme = scheme;
            _authority = authority;
        }

        public Url(string url, bool? addTrailingSlash = null)
        {
            _forcedAddTrailingSlashes = addTrailingSlash;
            if (url == null)
            {
                ClearUrl();
            }
            else
            {
                int queryIndex = QueryIndex(url);
                int hashIndex = url.IndexOf('#', queryIndex > 0 ? queryIndex : 0);
                int authorityIndex = url.IndexOf("://");

                if (queryIndex >= 0 && authorityIndex > queryIndex)
                {
                    authorityIndex = -1;
                }

                LoadFragment(url, hashIndex);
                LoadQuery(url, queryIndex, hashIndex);

                if (authorityIndex >= 0)
                {
                    LoadBasedUrl(url, queryIndex, hashIndex, authorityIndex);
                }
                else
                {
                    LoadSiteRelativeUrl(url, queryIndex, hashIndex);
                }
            }
        }

        public static bool AddTrailingSlashes { get; set; }
        public static bool LowercasePath { get; set; }

        void ClearUrl()
        {
            _scheme = null;
            _authority = null;
            _path = "";
            _query = null;
            _fragment = null;
        }

        void LoadSiteRelativeUrl(string url, int queryIndex, int hashIndex)
        {
            _scheme = null;
            _authority = null;
            if (queryIndex >= 0)
                _path = url.Substring(0, queryIndex);
            else if (hashIndex >= 0)
                _path = url.Substring(0, hashIndex);
            else if (url.Length > 0)
                _path = url;
            else
                _path = "";

            _path = CheckPath(_path);
        }

        void LoadBasedUrl(string url, int queryIndex, int hashIndex, int authorityIndex)
        {
            _scheme = url.Substring(0, authorityIndex);
            int slashIndex = url.IndexOf('/', authorityIndex + 3);
            if (slashIndex > 0)
            {
                _authority = url.Substring(authorityIndex + 3, slashIndex - authorityIndex - 3);
                if (queryIndex >= slashIndex)
                    _path = url.Substring(slashIndex, queryIndex - slashIndex);
                else if (hashIndex >= 0)
                    _path = url.Substring(slashIndex, hashIndex - slashIndex);
                else
                    _path = url.Substring(slashIndex);

                _path = CheckPath(_path);
            }
            else
            {
                _authority = url.Substring(authorityIndex + 3);
                _path = CheckPath("/");
            }
        }

        void LoadQuery(string url, int queryIndex, int hashIndex)
        {
            if (hashIndex >= 0 && queryIndex >= 0)
                _query = EmptyToNull(url.Substring(queryIndex + 1, hashIndex - queryIndex - 1));
            else if (queryIndex >= 0)
                _query = EmptyToNull(url.Substring(queryIndex + 1));
            else
                _query = null;
        }

        void LoadFragment(string url, int hashIndex)
        {
            if (hashIndex >= 0)
                _fragment = EmptyToNull(url.Substring(hashIndex + 1));
            else
                _fragment = null;
        }

        private string EmptyToNull(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            return text;
        }

        /// <summary>
        /// Хост
        /// </summary>
        public Url HostUrl
        {
            get { return new Url(_scheme, _authority, string.Empty, null, null, _forcedAddTrailingSlashes); }
        }

        /// <summary>
        /// Относительный путь
        /// </summary>
        public Url LocalUrl
        {
            get { return new Url(null, null, _path, _query, _fragment, _forcedAddTrailingSlashes); }
        }

        /// <summary>
        /// Scheme
        /// </summary>
        public string Scheme
        {
            get { return _scheme; }
        }

        /// <summary>
        /// Authority
        /// </summary>
        public string Authority
        {
            get { return _authority; }
        }

        /// <summary>
        /// Path
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// ПУть относительно корня приложения
        /// </summary>
        public string ApplicationRelativePath
        {
            get
            {
                string appPath = ApplicationPath;
                if (appPath.Equals("/"))
                    return "~" + Path;
                if (Path.StartsWith(appPath, StringComparison.InvariantCultureIgnoreCase))
                    return Path.Substring(appPath.Length);
                return Path;
            }
        }

        public string Query
        {
            get { return _query; }
        }

        public string Extension
        {
            get
            {
                int index = _path.LastIndexOfAny(dotsAndSlashes);

                if (index < 0)
                    return null;
                if (_path[index] == '/')
                    return null;

                return _path.Substring(index);
            }
        }

        public string PathWithoutExtension
        {
            get { return RemoveAnyExtension(_path); }
        }


        public string PathAndQuery
        {
            get { return string.IsNullOrEmpty(Query) ? Path : Path + "?" + Query; }
        }


        public string Fragment
        {
            get { return _fragment; }
        }


        public bool IsAbsolute
        {
            get { return _authority != null; }
        }

        public override string ToString()
        {
            string url;
            if (_authority != null)
                url = _scheme + "://" + _authority + CheckPath(_path);
            else
                url = _path;

            if (_query != null)
                url += "?" + _query;
            if (_fragment != null)
                url += "#" + _fragment;
            return url;
        }

        private string CheckPath(string path)
        {
            var newPath =  TrailingSlash(path);
            
            if(newPath == null)
                return null;

            if (LowercasePath)
            {
                newPath = newPath.ToLower();
            }

            return newPath;
        }

        private string TrailingSlash(string path)
        {
            if (path == null || path.Length == 0 /*|| (path.Length == 1 && path == "/")*/)
                return path;


            if ((_forcedAddTrailingSlashes.HasValue
                ? _forcedAddTrailingSlashes.Value
                : Url.AddTrailingSlashes) && !path.EndsWith("/"))
            {
                if (path.LastIndexOf("/") < path.LastIndexOf("."))
                {
                    return path;
                }
                return path + "/";
            }
            else if (_forcedAddTrailingSlashes.HasValue && !_forcedAddTrailingSlashes.Value)
            {
                return path.TrimEnd('/');
            }

            return path;
        }

        public static implicit operator string(Url u)
        {
            if (u == null)
                return null;
            return u.ToString();
        }

        public static implicit operator Url(string url)
        {
            return Parse(url);
        }


        public static string PathPart(string url)
        {
            url = RemoveHash(url);

            int queryIndex = QueryIndex(url);
            if (queryIndex >= 0)
                url = url.Substring(0, queryIndex);

            return url;
        }


        public static string QueryPart(string url)
        {
            url = RemoveHash(url);

            int queryIndex = QueryIndex(url);
            if (queryIndex >= 0)
                return url.Substring(queryIndex + 1);
            return string.Empty;
        }

        static int QueryIndex(string url)
        {
            return url.IndexOf('?');
        }


        public static string DefaultExtension
        {
            get { return defaultExtension; }
            set { defaultExtension = value; }
        }


        public static string DefaultDocument
        {
            get { return Url.defaultDocument; }
            set { Url.defaultDocument = value; }
        }


        public static string RemoveHash(string url)
        {
            if (url == null) return null;

            int hashIndex = url.IndexOf('#');
            if (hashIndex >= 0)
                url = url.Substring(0, hashIndex);
            return url;
        }


        public static Url Parse(string url, bool? addTrailingSlash = null)
        {
            if (url == null) return null;

            if (url.StartsWith("~"))
                url = ToAbsolute(url);

            return new Url(url, addTrailingSlash);
        }


        public static Url ParseTokenized(string url)
        {
            return Url.Parse(ResolveTokens(url));
        }

        public string GetQuery(string key)
        {
            IDictionary<string, string> queries = GetQueries();
            if (queries.ContainsKey(key))
                return queries[key];

            return null;
        }

        public string this[string queryKey]
        {
            get { return GetQuery(queryKey); }
        }

        public IDictionary<string, string> GetQueries()
        {
            return ParseQueryString(_query);
        }

        public Url AppendQuery(string key, string value)
        {
            return AppendQuery(key, value, true);
        }

        public Url AppendQuery(string key, string value, bool unlessNull)
        {
            if (unlessNull && value == null)
                return this;

            return AppendQuery(key + "=" + HttpUtility.UrlEncode(value));
        }

        public Url AppendQuery(string key, int value)
        {
            return AppendQuery(key + "=" + value);
        }

        public Url AppendQuery(string key, bool value)
        {
            return AppendQuery(key + (value ? "=true" : "=false"));
        }

        public Url AppendQuery(string key, object value)
        {
            if (value == null)
                return this;

            return AppendQuery(key + "=" + value);
        }

        public Url AppendQuery(string keyValue)
        {
            var clone = new Url(this);
            if (string.IsNullOrEmpty(_query))
                clone._query = keyValue;
            else if (!string.IsNullOrEmpty(keyValue))
                clone._query += Amp + keyValue;
            return clone;
        }

        public Url SetQueryParameter(string key, int value)
        {
            return SetQueryParameter(key, value.ToString());
        }

        public Url SetQueryParameter(string key, string value)
        {
            return SetQueryParameter(key, value, false);
        }

        public Url RemoveQuery(string key)
        {
            return SetQueryParameter(key, null, true);
        }

        public Url SetQueryParameter(string key, string value, bool removeNullValue)
        {
            if (removeNullValue && value == null && _query == null)
                return this;
            if (_query == null)
                return AppendQuery(key, value);

            var clone = new Url(this);
            string[] queries = _query.Split(querySplitter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < queries.Length; i++)
            {
                if (queries[i].StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (value != null)
                    {
                        queries[i] = key + "=" + HttpUtility.UrlEncode(value);
                        clone._query = string.Join(Amp, queries);
                        return clone;
                    }

                    if (queries.Length == 1)
                        clone._query = null;
                    else if (_query.Length == 2)
                        clone._query = queries[i == 0 ? 1 : 0];
                    else if (i == 0)
                        clone._query = string.Join(Amp, queries, 1, queries.Length - 1);
                    else if (i == queries.Length - 1)
                        clone._query = string.Join(Amp, queries, 0, queries.Length - 1);
                    else
                        clone._query = string.Join(Amp, queries, 0, i) + Amp + string.Join(Amp, queries, i + 1, queries.Length - i - 1);
                    return clone;
                }
            }
            return AppendQuery(key, value);
        }

        public Url SetQueryParameter(string keyValue)
        {
            if (_query == null)
                return AppendQuery(keyValue);

            int eqIndex = keyValue.IndexOf('=');
            if (eqIndex >= 0)
                return SetQueryParameter(keyValue.Substring(0, eqIndex), keyValue.Substring(eqIndex + 1));
            else
                return SetQueryParameter(keyValue, string.Empty);
        }

        public Url SetScheme(string scheme)
        {
            return new Url(scheme, _authority, _path, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url SetAuthority(string authority)
        {
            return new Url(_scheme ?? "http", authority, _path, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url SetPath(string path)
        {
            if (path.StartsWith("~"))
                path = ToAbsolute(path);
            int queryIndex = QueryIndex(path);
            return new Url(_scheme, _authority, queryIndex < 0 ? path : path.Substring(0, queryIndex), _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url SetQuery(string query)
        {
            return new Url(_scheme, _authority, _path, query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url SetExtension(string extension)
        {
            return new Url(_scheme, _authority, PathWithoutExtension + extension, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url SetFragment(string fragment)
        {
            if (fragment == null)
                return this;

            return new Url(_scheme, _authority, _path, _query, fragment.TrimStart('#'), _forcedAddTrailingSlashes);
        }

        public Url AppendSegment(string segment, string extension)
        {
            string newPath;
            if (string.IsNullOrEmpty(_path) || _path == "/")
                newPath = "/" + segment + extension;

            else if (!string.IsNullOrEmpty(extension))
            {
                int extensionIndex = _path.LastIndexOf(extension);
                if (extensionIndex >= 0)
                    newPath = _path.Insert(extensionIndex, "/" + segment);
                else if (_path.EndsWith("/"))
                    newPath = _path + segment + extension;
                else
                    newPath = _path + "/" + segment + extension;
            }
            else if (_path.EndsWith("/"))
                newPath = _path + segment;
            else
                newPath = _path + "/" + segment;

            return new Url(_scheme, _authority, newPath, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url AppendSegment(string segment)
        {
            if (string.IsNullOrEmpty(Path) || Path == "/")
                return AppendSegment(segment, DefaultExtension);

            return AppendSegment(segment, Extension);
        }

        public Url AppendSegment(string segment, bool useDefaultExtension)
        {
            return AppendSegment(segment, useDefaultExtension ? DefaultExtension : Extension);
        }

        public Url PrependSegment(string segment, string extension)
        {
            string newPath;
            if (string.IsNullOrEmpty(_path) || _path == "/")
            {
                newPath = "/" + segment + extension;
            }

            else if (extension != Extension)
            {
                if (!segment.EndsWith("/") && !_path.StartsWith("/"))
                    segment += "/";

                newPath = "/" + segment + PathWithoutExtension + extension;
            }
            else
            {
                if (!segment.EndsWith("/") && !_path.StartsWith("/"))
                    segment += "/";
                newPath = "/" + segment + _path;
            }

            return new Url(_scheme, _authority, newPath, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url PrependSegment(string segment)
        {
            if (string.IsNullOrEmpty(Path) || Path == "/")
            {
                return PrependSegment(segment, DefaultExtension);
            }

            return PrependSegment(segment, Extension);
        }

        public Url PrependSegment(string segment, bool addExtension)
        {
            if ((string.IsNullOrEmpty(Path) || Path == "/") && addExtension)
            {
                return PrependSegment(segment, DefaultExtension);
            }

            return PrependSegment(segment, Extension);
        }


        public Url UpdateQuery(NameValueCollection queryString)
        {
            Url u = new Url(this);
            foreach (string key in queryString.AllKeys)
            {
                u = u.SetQueryParameter(key, queryString[key]);
            }
            return u;
        }

        public Url UpdateQuery(IDictionary<string, string> queryString)
        {
            Url u = new Url(this);
            foreach (KeyValuePair<string, string> pair in queryString)
            {
                u = u.SetQueryParameter(pair.Key, pair.Value);
            }
            return u;
        }

        public Url UpdateQuery(IDictionary<string, object> queryString)
        {
            Url u = new Url(this);
            foreach (KeyValuePair<string, object> pair in queryString)
            {
                if (pair.Value != null)
                {
                    u = u.SetQueryParameter(pair.Key, pair.Value.ToString());
                }
            }
            return u;
        }


        public Url RemoveExtension()
        {
            return new Url(_scheme, _authority, PathWithoutExtension, _query, _fragment, _forcedAddTrailingSlashes);
        }

        public Url RemoveExtension(params string[] validExtensions)
        {
            var pathExtension = Array.Find(validExtensions, x => _path.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
            if (pathExtension == null)
            {
                return this;
            }
            return new Url(_scheme, _authority, _path.Substring(0, _path.Length - pathExtension.Length), _query, _fragment);
        }

        public Url RemoveDefaultDocument(string defualtDocument)
        {
            if (!_path.EndsWith("/" + defualtDocument, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            return new Url(_scheme, _authority, RemoveLastSegment(_path), _query, _fragment, _forcedAddTrailingSlashes);
        }

        private string GetLastSegment(string path)
        {
            int lastSegmentIndex = GetLastSignificatSlash(path);
            return path.Substring(lastSegmentIndex + 1);
        }

        public string GetLastSegment()
        {
            return GetLastSegment(ApplicationRelativePath);
        }

        public static string ToAbsolute(string path)
        {
            return ToAbsolute(ApplicationPath, path);
        }

        public static string ToAbsolute(string applicationPath, string path)
        {
            if (!string.IsNullOrEmpty(path) && path[0] == '~' && path.Length > 1)
            {
                return applicationPath + path.Substring(2);
            }
            else if (path == "~")
            {
                return applicationPath;
            }
            return path;
        }

        public static string ToRelative(string path, bool ignoreVirtualPath)
        {
            if (ignoreVirtualPath)
                return path;

            return ToRelative(ApplicationPath, path);
        }

        public static string ToRelative(string path)
        {
            return ToRelative(path, false);
        }

        public static string ToRelative(string applicationPath, string path)
        {
            if (!string.IsNullOrEmpty(path) && path.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
            {
                return "~/" + path.Substring(applicationPath.Length);
            }
            return path;
        }

        static string applicationPath;

        /// <summary>
        /// Адрес веб-приложения
        /// </summary>
        public static string ApplicationPath
        {
            get
            {
                if (applicationPath == null)
                {
                    try
                    {
                        applicationPath = VirtualPathUtility.ToAbsolute("~/");
                    }
                    catch
                    {
                        return "/";
                    }
                }
                return applicationPath;
            }
            set { applicationPath = value; }
        }

        /// <summary>
        /// Возвращает адрес без расширения
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveAnyExtension(string path)
        {
            int index = path.LastIndexOfAny(dotsAndSlashes);

            if (index < 0)
                return path;
            if (path[index] == '/')
                return path;

            return path.Substring(0, index);
        }

        /// <summary>
        /// Возвращает адрес без последнего фрагмента
        /// </summary>
        /// <param name="maintainExtension"></param>
        /// <returns></returns>
        public Url RemoveTrailingSegment(bool maintainExtension)
        {
            if (string.IsNullOrEmpty(_path) || _path == "/")
                return this;

            string newPath = "/";

            int lastSlashIndex = _path.LastIndexOf('/');
            if (lastSlashIndex == _path.Length - 1)
                lastSlashIndex = _path.TrimEnd(slashes).LastIndexOf('/');
            if (lastSlashIndex > 0)
                newPath = _path.Substring(0, lastSlashIndex) + (maintainExtension ? Extension : "");

            return new Url(_scheme, _authority, newPath, _query, _fragment, _forcedAddTrailingSlashes);
        }

        /// <summary>
        /// Возвращает адрес без сегмента
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Url RemoveTrailingSegment()
        {
            return RemoveTrailingSegment(true);
        }

        /// <summary>
        /// Возвращает адрес без сегмента
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Url RemoveSegment(int index)
        {
            if (string.IsNullOrEmpty(_path) || _path == "/" || index < 0)
                return this;

            if (index == 0)
            {
                int slashIndex = _path.IndexOf('/', 1);
                if (slashIndex < 0)
                    return new Url(_scheme, _authority, "/", _query, _fragment, _forcedAddTrailingSlashes);
                return new Url(_scheme, _authority, _path.Substring(slashIndex), _query, _fragment, _forcedAddTrailingSlashes);
            }

            string[] segments = PathWithoutExtension.Split(slashes, StringSplitOptions.RemoveEmptyEntries);
            if (index >= segments.Length)
                return this;

            if (index == segments.Length - 1)
                return RemoveTrailingSegment();

            string newPath = "/" + string.Join("/", segments, 0, index) + "/" + string.Join("/", segments, index + 1, segments.Length - index - 1) + Extension;
            return new Url(_scheme, _authority, newPath, _query, _fragment, _forcedAddTrailingSlashes);
        }

        /// <summary>
        /// Возвращает массив сегментов адреса
        /// </summary>
        /// <returns></returns>
        public string[] GetSegments()
        {
            return PathWithoutExtension.Split(slashes, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string RemoveLastSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            int slashIndex = GetLastSignificatSlash(path);
            return path.Substring(0, slashIndex + 1);
        }

        public static string GetName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            int slashIndex = GetLastSignificatSlash(path);
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == slashIndex)
                return path.Substring(slashIndex + 1);

            return path.Substring(slashIndex + 1, lastSlashIndex - slashIndex - 1);
        }

        private static int GetLastSignificatSlash(string path)
        {
            int i = path.Length - 1;
            for (; i >= 0; i--)
            {
                if (path[i] != '/')
                    break;
            }
            for (; i >= 0; i--)
            {
                if (path[i] == '/')
                    break;
            }
            return i;
        }

        /// <summary>
        /// Декодирует адрес
        /// </summary>
        /// <returns></returns>
        public string Encode()
        {
            // TODO: требует доработки
            return ToString().Replace(Amp, "&amp;");
        }

        /// <summary>
        /// Возвращает слова
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ParseQueryString(string query)
        {
            var dictionary = new Dictionary<string, string>();
            if (query == null)
                return dictionary;

            string[] queries = query.Split(querySplitter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < queries.Length; i++)
            {
                string q = queries[i];
                int eqIndex = q.IndexOf("=");
                if (eqIndex >= 0)
                    dictionary[q.Substring(0, eqIndex)] = q.Substring(eqIndex + 1);
            }
            return dictionary;
        }

        public static string Rebase(string currentPath, string fromAppPath, string toAppPath)
        {
            if (currentPath == null || fromAppPath == null || !currentPath.StartsWith(fromAppPath))
                return currentPath;

            return toAppPath + currentPath.Substring(fromAppPath.Length);
        }

        public static string GetToken(string token)
        {
            string value = null;
            replacements.TryGetValue(token, out value);
            return value;
        }

        public static void SetToken(string token, string value)
        {
            if (token == null) throw new ArgumentNullException("key");

            if (value != null)
            {
                replacements[token] = value;
            }
            else if (replacements.ContainsKey(token))
            {
                replacements.Remove(token);
            }
        }

        public static string ResolveTokens(string urlFormat)
        {
            if (string.IsNullOrEmpty(urlFormat))
            {
                return urlFormat;
            }

            foreach (var kvp in replacements)
            {
                urlFormat = urlFormat.Replace(kvp.Key, kvp.Value);
            }
            return ToAbsolute(urlFormat);
        }

        public Url ResolveTokens()
        {
            return new Url(ResolveTokens(ToString()), _forcedAddTrailingSlashes);
        }
    }
}
