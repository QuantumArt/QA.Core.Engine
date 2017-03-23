// Owners: Karlov Nikolay

using System;

namespace QA.Core.Engine
{
    public interface IControllerMapper
    {
        string GetControllerName(Type type);

        bool ControllerHasAction(string controllerName, string actionName);
    }
}
