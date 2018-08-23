
#pragma warning disable 1591

namespace QA.Core.Engine.Web
{
    public class PageNotFoundEventArgs : ItemEventArgs
    {
        private string url;
        PathData affectedPath;

        public PageNotFoundEventArgs(string url)
            : base(null)
        {
            this.url = url;
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public PathData AffectedPath
        {
            get { return affectedPath; }
            set { affectedPath = value; }
        }
    }
}
