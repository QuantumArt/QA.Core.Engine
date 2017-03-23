using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using QA.Core;
using QA.Core.Engine;
using QA.Core.Engine.Extensions.Html;
using QA.Core.Engine.UI;
using QA.Core.Engine.Web;
using QA.Core.Web;
using QA.Core.Web.Qp;

namespace QA.Engine.Extensions.Html
{
    /// <summary>
    /// Панель редактирования сайта
    /// </summary>
    public class ControlPanelHelper
    {
        private HtmlHelper _helper;
        private AbstractItem _contentItem;
        private ZoneState _zoneState;
        private static string _version = "v" + DateTime.Now.Ticks.ToString();

        public ControlPanelHelper(HtmlHelper helper, AbstractItem contentItem)
        {
            _helper = helper;
            _contentItem = contentItem;
            _zoneState = ZoneExtensions.GetControlState(helper.ViewContext.RequestContext.HttpContext);
        }

        /// <summary>
        /// Регистрация стилей и скриптов панели управления виджетами.
        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            // регистрируем стили и скрипты
            bundles.Add(new EmbeddedResourceBundle(ControlPanelResourcePath.ControlPanelStyleVirtualPath,
              EmbeddedResourceBundle.ContentType.TextCss)
                    .IncludeEmbedded(ControlPanelResourcePath.ControlPanelCSSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.EditorWrapperCSSPath, typeof(ControlPanelResourcePath)));

            bundles.Add(new EmbeddedResourceBundle(ControlPanelResourcePath.ControlPanelScriptVirtualPath,
                EmbeddedResourceBundle.ContentType.TextJavaScript)
                    .IncludeEmbedded(ControlPanelResourcePath.QP8BackendApiInteractionJSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.QPUtilsJSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.QAEngineEditingJSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.ControlPanelJSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.WidgetItemJSPath, typeof(ControlPanelResourcePath))
                    .IncludeEmbedded(ControlPanelResourcePath.QAEngineContentEditorWrapper, typeof(ControlPanelResourcePath))
                    );
        }

        public virtual void Render()
        {
            RenderResources();

            if (_contentItem != null)
            {
                Render(_helper.ViewContext.Writer);
            }
            else
            {
                // можно рендерить только глобальные зоны
            }
        }

        public void RenderResources()
        {
            if (_zoneState == ZoneState.Editing || _zoneState == ZoneState.Visible)
            {
                RenderHostId();
                using (TagWrapper.Begin("script", _helper.ViewContext.Writer, new { type = "text/javascript" }))
                {
                    _helper.ViewContext.Writer.WriteLine(GetManagmentUrlScript(_zoneState));
                }

                if (!_helper.ViewContext.HttpContext.IsDebuggingEnabled)
                {
                    // TODO: render styles and scripts
                    _helper.ViewContext
                        .Writer.WriteLine(
                            Styles.Render(ControlPanelResourcePath.ControlPanelStyleVirtualPath).ToHtmlString());

                    _helper.ViewContext
                        .Writer
                        .WriteLine(Scripts
                            .Render(ControlPanelResourcePath
                                .ControlPanelScriptVirtualPath).ToHtmlString());
                }
                else
                {
                    // в режиме debug скрипты е сжимаются, bundle выдаёт пустой результат
                    var urlHelper = new UrlHelper(_helper.ViewContext.RequestContext);

                    _helper.ViewContext.Writer.WriteLine(
                        string.Format(@"<link rel='stylesheet' type='text/css' href='{0}' />",
                        urlHelper.Content(
                            new Url(ControlPanelResourcePath.ControlPanelStyleVirtualPath)
                            .AppendQuery(_version))));

                    _helper.ViewContext.Writer.WriteLine(
                        string.Format(@"<script src='{0}' type='text/javascript'></script>",
                        urlHelper.Content(
                            new Url(ControlPanelResourcePath.ControlPanelScriptVirtualPath)
                            .AppendQuery(_version))));
                }
            }
        }

        public void RenderHostId()
        {
            var hostId = QpHelper.HostId;
            if (!string.IsNullOrEmpty(hostId))
            {
                using (TagWrapper.Begin("Script", _helper.ViewContext.Writer, new { type = "text/javascript" }))
                {
                    _helper.ViewContext.Writer.WriteLine(@"
    if(window.sessionStorage) {
        sessionStorage.setItem('7C46280A-A276-4F1A-8FA3-053B38959E09', '" + hostId + @"');
    }
    window.name = '" + hostId + @"';
");
                }

            }
        }

        private void Render(TextWriter textWriter)
        {
            if (_zoneState == ZoneState.Editing || _zoneState == ZoneState.Visible)
            {
                var utilities = ObjectFactoryBase.Resolve<PartUtilities>();
                using (TagWrapper.Begin("div", textWriter, new { @class = "control-panel" }))
                {
                    WriteControls(textWriter, utilities);

                    if (_zoneState == ZoneState.Editing)
                    {
                        WriteDefinitions(textWriter);
                        using (TagWrapper.Begin("div", textWriter, new { @class = "controls-bottom" }))
                        {
                            PartUtilities.WriteCommand(textWriter, "done", "command done",
                                Url.Parse(Url.Parse(
                                        _helper
                                        .ViewContext
                                        .HttpContext
                                        .Request
                                        .Url.ToString())
                                    .RemoveQuery("backend_sid")
                                    .RemoveQuery("hostUID"))
                                    .RemoveQuery("editing"));
                        }
                    }
                }
            }
        }

        private void WriteControls(TextWriter textWriter, PartUtilities utilities)
        {
            using (TagWrapper.Begin("div", textWriter, new { @class = "control-panel-controls" }))
            {
                utilities.WriteControlPanel(textWriter, _contentItem, _helper.ViewContext.RequestContext.HttpContext.Request.Url.ToString());
            }
        }

        private void WriteDefinitions(TextWriter textWriter)
        {
            // TODO: get all definitions
            var definitions = DefinitionsHelper
                .GetAllowedDefinitions(_contentItem, _helper.ViewContext.HttpContext.User)
                .GroupBy(x => x.Category);

            foreach (var group in definitions)
            {
                using (TagWrapper.Begin("div", textWriter, new { @class = "group collapsed" }))
                {
                    using (TagWrapper.Begin("div", textWriter, new { @class = "expander" }))
                    {
                        using (TagWrapper.Begin("span", textWriter, new { @class = "expander-icon" }))
                        {
                            textWriter.Write("");
                        }
                        using (TagWrapper.Begin("span", textWriter, new { @class = "expander-header" }))
                        {
                            textWriter.Write(group.Key ?? "Основные виджеты");
                        }
                        using (TagWrapper.Begin("div", textWriter, new { @class = "definitions" }))
                        {
                            foreach (var definition in group)
                            {
                                if (definition.IsPage)
                                {
                                    continue;
                                }

                                WriteDefinition(textWriter, definition);
                            }
                        }
                    }
                }
            }
        }

        private static void WriteDefinition(TextWriter textWriter, ItemDefinition definition)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add(PartUtilities.ZoneAttribute, string.Join(" ", definition.AllowedZoneNames));
            attrs.Add(PartUtilities.TypeAttribute, definition.Discriminator);
            attrs.Add(PartUtilities.TitleAttribute, definition.Title);
            attrs.Add(PartUtilities.IconUrlAttribute, definition.IconUrl);
            attrs.Add("class", "definition-draggable");

            var spanAttrs = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(definition.IconUrl))
            {
                spanAttrs.Add("style",
                    string.Format("background: url('{0}') no-repeat;", definition.IconUrl));

                spanAttrs.Add("class", "definition-title");
            }

            using (TagWrapper.Begin("div", textWriter, attrs))
            {
                using (TagWrapper.Begin("span", textWriter, spanAttrs))
                {
                    textWriter.Write("&nbsp;");
                }
                using (TagWrapper.Begin("span", textWriter, new { @class = "item-definition title definition-header" }))
                {
                    textWriter.Write(definition.Title);
                }
                using (TagWrapper.Begin("div", textWriter, new { @class = "item-definition description" }))
                {
                    textWriter.Write(definition.Description);
                }
            }
        }

        private string GetManagmentUrlScript(ZoneState state)
        {
            return string.Format(
@"
var ContentIdToEdit = {2};
var CurrentReturnUrl = '{3}';
var Control_Panel_Managment_Url = '{0}';
var Control_Panel_Managment_Mode = '{1}'",
                ObjectFactoryBase
                    .Resolve<IEditUrlManager>()
                    .GetBaseNavigationUrl(),
                    state,
                    293, /* TODO: брать из конфига или маппинга*/
                    Url.Parse(_helper.ViewContext.HttpContext.Request.Url.ToString()).RemoveQuery("backend_sid").RemoveQuery("hostUID"));
        }
    }
}
