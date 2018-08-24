using System.Web.Mvc;
using System.Web;
using System;
using QA.Core.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Web.Mvc
{
    /// <summary>
    /// Base controller for all widgets and pages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ContentControllerBase<T> : QAControllerBase
        where T : AbstractItem
    {
        AbstractItem _currentPart;
        T _currentItem;
        AbstractItem _currentPage;
        IEngine _engine;

        public virtual IEngine Engine
        {
            get
            {
                return _engine
                    ?? (_engine = RouteExtensions.GetEngine(RouteData));
            }
            set { _engine = value; }
        }

        public virtual T CurrentItem
        {
            get
            {
                return _currentItem
                    ?? (_currentItem = ControllerContext.RequestContext.CurrentItem<T>());
            }
            set { _currentItem = value; }
        }

        public AbstractItem CurrentPage
        {
            get
            {
                return _currentPage
                  ?? (_currentPage = ControllerContext.RequestContext.CurrentPage()
                      ?? Engine.UrlParser.CurrentPage
                      ?? CurrentItem.ClosestPage());
            }
            set { _currentPage = value; }
        }

        public AbstractItem CurrentPart
        {
            get
            {
                return _currentPart
                    ?? (_currentPart = ControllerContext.RequestContext.CurrentPart<AbstractItem>());
            }
            set { _currentPart = value; }
        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MergeModelStateFromEarlierStep(filterContext.HttpContext);

            ViewData["CurrentCultureToken"] = RouteData.DataTokens["CurrentCultureToken"];
            ViewData["CurrentRegionToken"] = RouteData.DataTokens["CurrentRegionToken"];


            ViewData[ContentRoute.ContentPageKey] = CurrentPage;
            ViewData[ContentRoute.AbstractItemKey] = CurrentItem;
            ViewData[ContentRoute.StartPageKey] = Engine.UrlParser.StartPage;
            ViewData[ContentRoute.RootPageKey] = Engine.UrlParser.RootPage;
            base.OnActionExecuting(filterContext);
        }


        private void MergeModelStateFromEarlierStep(HttpContextBase context)
        {
            const string modelStateForMergingKey = "ModelStateForMerging{FC3D6636-EE37-45FA-A461-B084FD07B285}";

            ModelStateDictionary modelState = ModelState;
            if (context.Items.Contains(modelStateForMergingKey))
            {
                modelState = context.Items[modelStateForMergingKey] as ModelStateDictionary;

                if (modelState != null)
                    ModelState.Merge(modelState);
            }

            context.Items[modelStateForMergingKey] = modelState;
        }

        protected virtual ActionResult RedirectToParentPage()
        {
            return Redirect(CurrentPage.Url);
        }

        protected internal virtual ViewPageResult ViewParentPage()
        {
            if (CurrentItem != null && CurrentItem.IsPage)
            {
                throw new InvalidOperationException(
                    "The current page is already being rendered. ViewParentPage should only be used from content items to render their parent page.");
            }

            return ViewPage(CurrentPage);
        }

        protected internal virtual ViewPageResult ViewPage(AbstractItem thePage)
        {
            if (thePage == null)
                throw new ArgumentNullException("thePage");

            if (!thePage.IsPage)
                throw new InvalidOperationException("Item " + thePage.GetContentType().Name + " is not a page type and cannot be rendered on its own.");

            if (thePage == CurrentItem)
                throw new InvalidOperationException("The page passed into ViewPage was the current page. This would cause an infinite loop.");

            var mapper = Engine.Resolve<IControllerMapper>();

            return new ViewPageResult(thePage, mapper, ActionInvoker);
        }

    }
}
