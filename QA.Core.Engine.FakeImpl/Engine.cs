
using System;
using Microsoft.Practices.Unity;
using QA.Core.Engine.Web;

namespace QA.Core.Engine.Data
{
    [Obsolete("Use Engine instead")]
    public class FakeEngine : WidgetSystemEngine
    {
        public FakeEngine(IUnityContainer container) : base(container)
        {
        }
    }


    public class WidgetSystemEngine : IEngine
    {
        public WidgetSystemEngine(IUnityContainer container)
        {
            _container = container;
        }

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

        public IControllerMapper ControllerMapper
        {
            get
            {
                return _container.Resolve<IControllerMapper>();
            }
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public IEngine RegisterAssemblyWithType(Type typeExportedByAsseblyToRegister)
        {
            Resolve<ITypeFinder>().RegisterAssemblyWithType(typeExportedByAsseblyToRegister);
            return this;
        }
    }
}
