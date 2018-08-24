using System;
using System.Collections.Generic;
using System.Web.Mvc;
using QA.Core.Engine;
using QA.Core.Engine.Web.Mvc;
using QA.Core.Web;
using QA.Engine.Extensions.Html;
#pragma warning disable 1591

namespace QA.Engine.Extensions.Html
{
    public class ContentEditorWrapper : IDisposable
    {
        private string _actionName;
        private int _contentId;
        private HtmlHelper _helper;
        private Stack<IDisposable> _stack;
        private EditingActions _availableActions;
        private System.IO.TextWriter _writer;
        private Guid _seed;
        private int _objectId;
        private bool _isBlocked;

        public ContentEditorWrapper(HtmlHelper helper, EditingActions availableActions, string title)
        {
            _stack = new Stack<IDisposable>();
            var zoneState = ZoneExtensions.GetZoneState(helper.ViewContext.HttpContext);

            _seed = Guid.NewGuid();
            _helper = helper;
            _availableActions = availableActions;
            _writer = helper.ViewContext.Writer;

            if (zoneState != ZoneState.Visible)
            {
                _isBlocked = true;
                return;
            }

            _stack.Push(TagWrapper.Begin("div", _writer, new { @class = "contentEditorWrapper" }));

            using (TagWrapper.Begin("div", _writer, new { @class = "contentEditorControls" }))
            {
                using (TagWrapper.Begin("span", _writer, new { @class = "contentEditorWrapper-title" }))
                {
                    _writer.Write(title ?? "OnScreen");
                }

                // write controls
                if ((_availableActions & EditingActions.Edit) != 0)
                {
                    RenderActionButton(EditingActions.Edit, "ред.");
                }

                if ((_availableActions & EditingActions.Navigate) != 0)
                {
                    RenderActionButton(EditingActions.Navigate, "св-ва");
                }

                if ((_availableActions & EditingActions.Delete) != 0)
                {
                    RenderActionButton(EditingActions.Delete, "удал.");
                }
            }
            _stack.Push(TagWrapper.Begin("div", _writer, new { @class = "contentEditorWrapper-body" }));

        }

        private void RenderActionButton(EditingActions action, string actionName)
        {
            using (TagWrapper.Begin("span", _writer, new { @class = "editor-action " + action.ToString().ToLower(), id = GetUniqueId(action.ToString()) }))
            {
                _writer.Write(actionName);
            }
        }

        public ContentEditorWrapper WithActionName(string actionName)
        {
            _actionName = actionName;
            return this;
        }

        public ContentEditorWrapper WithContentId(int contentId)
        {
            _contentId = contentId;
            return this;
        }

        public ContentEditorWrapper WithArticleId(int articleId)
        {
            _objectId = articleId;
            return this;
        }

        public void Dispose()
        {
            if (!_isBlocked)
            {
                using (TagWrapper.Begin("script", _writer, new { type = "text/javascript" }))
                {
                    _writer.Write(string.Format(@"
                    $(function(){{
                        // var contentId = {0}; var objectId = {1}; var returnUrl = {2};
                        // Edit
                        $('#{3}').click(function(e) {{ e.preventDefault(); QA.Engine.ContentEditorWrapper.editQPArticle({0}, {1}, {2}); return false; }});
                        // Navigate
                        $('#{4}').click(function(e) {{ e.preventDefault(); QA.Engine.ContentEditorWrapper.showQPArticle({0}, {1}, {2}); return false; }});
                        // Delete
                        $('#{5}').click(function(e) {{ e.preventDefault(); QA.Engine.ContentEditorWrapper.deleteQPArticle({0}, {1}, {2}); return false; }});
                    }});",
                             _objectId,
                             _contentId,
                             GetUrl(),
                             GetUniqueId(EditingActions.Edit.ToString()),
                             GetUniqueId(EditingActions.Navigate.ToString()),
                             GetUniqueId(EditingActions.Delete.ToString())));
                }
            }

            while (_stack.Count > 0)
            {
                // закрываем теги
                _stack.Pop().Dispose();
            }
        }

        private string GetUrl()
        {
            if (_helper.ViewContext.HttpContext.Request.IsAjaxRequest())
            {
                var item = GetCurrentItem();

                if (item != null && item.IsPage)
                {
                    return string.Format("'{0}'", item.Url);
                }
            }
            else
            {
                return string.Format("'{0}'", new Url(_helper.ViewContext.HttpContext.Request.Url.ToString())
                    .RemoveQuery("backend_sid")
                    .RemoveQuery("hostUID"));
            }
            return "null";
        }


        protected virtual AbstractItem GetCurrentItem()
        {
            return ((AbstractItem)_helper.ViewContext.ViewData[ContentRoute.ContentPageKey] ??
                (AbstractItem)_helper.ViewContext.ViewData[ContentRoute.AbstractItemKey] ??
                (AbstractItem)_helper.ViewContext.ViewData["CurrentPage"]) ??
                _helper.ViewContext.Controller.ControllerContext.RequestContext.CurrentPage();
        }

        private string GetUniqueId(string id)
        {
            return string.Format("{0}_{1}", id, _seed);
        }
    }

    [Flags]
    public enum EditingActions
    {
        /// <summary>
        /// Ничего не делать
        /// </summary>
        None = 0,

        /// <summary>
        /// Редактирование
        /// </summary>
        Edit = 1,

        /// <summary>
        /// Навигация
        /// </summary>
        Navigate = 2,

        /// <summary>
        /// Удаление
        /// </summary>
        Delete = 4,

        /// <summary>
        /// Все
        /// </summary>
        All = (Edit | Navigate)
    }
}
