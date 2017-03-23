using System;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace QA.Core.Engine.Details
{
    public class EditableCheckBoxAttribute : AbstractEditableAttribute
    {
        public EditableCheckBoxAttribute(string title) : base(title) { }

        protected override TagBuilder CreateEditor(object item, object value)
        {
            TagBuilder container = new TagBuilder("div");

            TagBuilder input = new TagBuilder("input");

            var name = GetNameForProperty();
            bool objValue = false;

            if (item != null)
            {
                objValue = value != null ? Convert.ToBoolean(value) : false;
            }

            input.Attributes.Add("type", "checkbox");
            input.Attributes.Add("name", name);
            input.GenerateId(name);

            if (objValue.Equals(true))
            {
                input.Attributes.Add("checked", "checked");
            }

            container.AddCssClass(this.GetType().Name);
            container.InnerHtml = input.ToString();

            return container;
        }

        protected override object ExtractValue(NameValueCollection form)
        {
            var name = GetNameForProperty();
            var value = form[name];

            if (value != null)
            {
                // TODO: Parse exact
               return value != null ? (
                    value.Equals("on", StringComparison.InvariantCultureIgnoreCase) ||
                    value.Equals("checked", StringComparison.InvariantCultureIgnoreCase) ||
                    value.Equals("true", StringComparison.InvariantCultureIgnoreCase)||
                    // in case of @Html.CheckboxFor(x => ...)
                    value.ToLower().Contains("true")
                    ):(false);
            }

            return null;
        }


        protected override bool OnValidation(out string error, object value)
        {
            error = null;
            return true;
        }
    }
}
