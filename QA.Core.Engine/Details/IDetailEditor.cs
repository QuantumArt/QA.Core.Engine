using System.Collections.Specialized;
using System.Reflection;
using System.Web.Mvc;

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
