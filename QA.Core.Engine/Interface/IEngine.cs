using System;
using QA.Core.Engine.Web;

namespace QA.Core.Engine
{
    public interface IEngine
    {
        IUrlParser UrlParser { get; }
        IPersister Persister { get; }
        IControllerMapper ControllerMapper { get; }
        T Resolve<T>();
        IEngine RegisterAssemblyWithType(Type typeExportedByAsseblyToRegister);
    }
}
