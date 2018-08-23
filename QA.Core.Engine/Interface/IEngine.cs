using System;
using QA.Core.Engine.Web;
#pragma warning disable 1591

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
