using System;
using System.Collections.Specialized;
using System.Web.Mvc;
#pragma warning disable 1591

namespace QA.Core.Engine.Details
{
    public class EditableTextAreaAttribute : EditableTextAttribute
    {
        public int Columns { get; set; }
        public int Rows { get; set; }
        public EditableTextAreaAttribute(string title)
            : base(title)
        {
            Columns = 10;
            Rows = 3;
        }

        protected override TagBuilder CreateEditor(object item, object value)
        {
            TagBuilder container = new TagBuilder("div");

            TagBuilder input = new TagBuilder("textarea");

            var name = GetNameForProperty();

            string objValue = null;

            if (item != null)
            {
                objValue = value != null ? Convert.ToString(value) : null;
            }

            input.Attributes.Add("name", name);
            input.GenerateId(name);

            if (value != null)
            {
                input.InnerHtml = objValue;
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
                return Convert.ChangeType(value, PropertyType);
            }

            return null;
        }
    }
}
