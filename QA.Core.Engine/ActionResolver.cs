using System;

namespace QA.Core.Engine
{
    public class ActionResolver : IPathFinder
    {
        protected readonly IControllerMapper _controllerMapper;
        protected readonly string[] _methods;

        public ActionResolver(IControllerMapper controllerMapper, string[] methods)
        {
            this._controllerMapper = controllerMapper;
            this._methods = methods;
        }

        public string[] Methods
        {
            get { return _methods; }
        }

        public virtual PathData GetPath(AbstractItem item, string remainingUrl)
        {
            int slashIndex = remainingUrl.IndexOf('/');

            string action = remainingUrl;
            string arguments = null;
            if (slashIndex > 0)
            {
                action = remainingUrl.Substring(0, slashIndex);
                arguments = remainingUrl.Substring(slashIndex + 1);
            }

            var controllerName = _controllerMapper.GetControllerName(item.GetContentType());
            if (string.IsNullOrEmpty(action) || string.Equals(action, "Default.aspx", StringComparison.InvariantCultureIgnoreCase))
                action = "Index";

            foreach (string method in _methods)
            {
                if (string.Equals(method, action, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new PathData(item, null, action, arguments)
                    {
                        IsRewritable = false,
                        TemplateUrl = string.Format("~/{0}/{1}", controllerName, method, item.Id) // workaround for start pages
                    };
                }
            }

            return null;
        }
    }
}
