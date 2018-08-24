using System.Collections.Specialized;
using System.Reflection;
using System.Web.Mvc;
#pragma warning disable 1591

namespace QA.Core.Engine.Details
{
    public interface IDetailEditor : ITitled, IOrdered
    {
        void Attach(AbstractItem item, PropertyInfo propertyInfo);
        TagBuilder CreateEditor(object item);
        bool TrySetValue(NameValueCollection form, ModelStateDictionary modelState);
        string PropertyName { get; }
    }
}
