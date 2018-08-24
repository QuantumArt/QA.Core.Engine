using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using QA.Core.Collections;
#pragma warning disable 1591

namespace QA.Core.Engine.Web
{
    /// <summary>
    /// Bundle, позволяющий публиковать файлы, являющиеся embedded resource
    /// </summary>
    public class EmbeddedResourceBundle : Bundle
    {
        public enum ContentType
        {
            TextJavaScript, TextCss
        }

        Dictionary<string, Assembly> _resources;
        private ContentType _contentType;

        public ReadOnlyDictionary<string, Assembly> Resources
        {
            get;
            private set;
        }

        public EmbeddedResourceBundle(string path, ContentType contentType) :
            base(path, contentType == ContentType.TextJavaScript ?
                (IBundleTransform)new JsMinify() : (IBundleTransform)new CssMinify())
        {
            _resources = new Dictionary<string, Assembly>();
            Resources = new ReadOnlyDictionary<string, Assembly>(_resources);
            _contentType = contentType;
        }

        public override BundleResponse GenerateBundleResponse(BundleContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in Resources.Keys)
            {
                try
                {
                    using (var resourceStream = Resources[key].GetManifestResourceStream(key))
                    {
                        using (var reader = new StreamReader(resourceStream))
                        {
                            sb.AppendLine(reader.ReadToEnd());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    var logger = ObjectFactoryBase.Logger;

                    if (logger != null)
                    {
                        if (logger != null)
                        {
                            logger.ErrorException("Error while bundling", ex);
                        }
                    }

                    sb.AppendLine("/* error in bundling */");
                }
            }

            var response = new BundleResponse(sb.ToString(), this.EnumerateFiles(context));

            switch (_contentType)
            {
                case ContentType.TextJavaScript:
                    response.ContentType = "text/javascript";
                    break;
                case ContentType.TextCss:
                    response.ContentType = "text/css";
                    break;
                default:
                    break;
            }

            response.Cacheability = HttpCacheability.ServerAndPrivate;

            if (!context.HttpContext.IsDebuggingEnabled)
            {
                var transform = Transforms[0];
                transform.Process(context, response);
            }

            return response;
        }

        public EmbeddedResourceBundle IncludeEmbedded(string virtualPath, Type type)
        {
            Throws.IfArgumentNull(type, _ => type);
            Throws.IfArgumentNullOrEmpty(virtualPath, _ => virtualPath);

            _resources.Add(virtualPath, type.Assembly);

            return this;
        }
    }
}
