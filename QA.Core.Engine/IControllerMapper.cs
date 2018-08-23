// Owners: Karlov Nikolay

using System;
using System.Collections.Generic;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    public interface IControllerMapper
    {
        string GetControllerName(Type type);

        bool ControllerHasAction(string controllerName, string actionName);

        IEnumerable<string> GetActionsFor<T>() where T : AbstractItem;

        IEnumerable<string> GetActionsForItemType(Type itemType);
    }
}
