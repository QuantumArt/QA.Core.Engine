using System.Web.Mvc;
using System.Web;
using System;
using QA.Core.Web;

namespace QA.Core.Engine.Web.Mvc
{
    /// <summary>
    /// Base controller for widgets and pages with abstract method Index()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ContentController<T> : ContentControllerBase<T>
        where T : AbstractItem
    {
        public abstract ActionResult Index();
    }
}
