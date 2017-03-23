using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Engine.Data
{
    public class FakeControllerMapper : IControllerMapper
    {
        public string GetControllerName(Type type)
        {
            throw new NotImplementedException();
        }

        public bool ControllerHasAction(string controllerName, string actionName)
        {
            throw new NotImplementedException();
        }
    }
}
