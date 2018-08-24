using System;
using System.Collections.Specialized;
using System.Web.Mvc;
#pragma warning disable 1591

namespace QA.Core.Engine.Details
{
    public class EditableTextBoxAttribute : EditableTextAttribute
    {
        public EditableTextBoxAttribute(string title) : base(title) { }

        protected override TagBuilder CreateEditor(object item, object value)
        {
            TagBuilder input = new TagBuilder("input");

            var name = GetNameForProperty();

            string objValue = null;

            if (item != null)
            {
                objValue = value != null ? Convert.ToString(value) : null;
            }

            input.Attributes.Add("type", "textbox");
            input.Attributes.Add("name", name);
            input.GenerateId(name);

            if (value != null)
            {
                input.Attributes.Add("value", objValue);
            }

            return input;
        }

        protected override object ExtractValue(NameValueCollection form)
        {
            var name = GetNameForProperty();
            var value = form[name];

            if (value != null)
            {
                // TODO: Parse exact
                return Convert.ChangeType(value, PropertyType);
            }

            return null;
        }
    }
}
