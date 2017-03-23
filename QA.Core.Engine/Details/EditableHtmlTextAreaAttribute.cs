using System;
using System.Web.Mvc;

namespace QA.Core.Engine.Details
{
    public class EditableHtmlTextAreaAttribute : EditableTextAreaAttribute
    {
        public EditableHtmlTextAreaAttribute(string title)
            : base(title)
        {

        }

        protected override TagBuilder CreateEditor(object item, object value)
        {
            TagBuilder container = new TagBuilder("div");
            TagBuilder input = new TagBuilder("textarea");

            var id = GetUniqueId();

            var name = GetNameForProperty();
            string objValue = null;

            if (item != null)
            {
                objValue = value != null ? Convert.ToString(value) : null;
            }

            input.Attributes.Add("name", name);

            // TODO: user configurable constants
            input.AddCssClass("ckeditor");

            if (value != null)
            {
                input.InnerHtml = objValue;
            }

            container.AddCssClass(this.GetType().Name);
            container.InnerHtml = input.ToString();

            return container;
        }
    }
}
