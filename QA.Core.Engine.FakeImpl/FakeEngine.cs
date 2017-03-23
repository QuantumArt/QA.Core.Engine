
using Microsoft.Practices.Unity;
using QA.Core.Engine.Web;

namespace QA.Core.Engine.Data
{
    public class FakeEngine : IEngine
    {
        public FakeEngine(IUnityContainer container)
        {
            _container = container;
        }
        private IPersister _persister;
        private IUnityContainer _container;
        public IUrlParser UrlParser
        {
            get
            {
                return _container.Resolve<IUrlParser>();
            }
        }

        public IPersister Persister
        {
            get
            {
                return _container.Resolve<IPersister>();
            }
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
