// Owners: Karlov Nikolay

using System;
using System.Collections.Generic;

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
