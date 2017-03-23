using QA.Core.Engine.Web;

namespace QA.Core.Engine
{
    public interface IEngine
    {
        IUrlParser UrlParser { get; }
        IPersister Persister { get; }
        T Resolve<T>();
    }
}
