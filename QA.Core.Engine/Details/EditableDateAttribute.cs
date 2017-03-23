using System;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace QA.Core.Engine.Details
{
    public class EditableDateAttribute : AbstractEditableAttribute
    {
        public string DateFormat { get; set; }

        protected const string dateFormat = @"yy.mm.dd";

        public EditableDateAttribute(string title) : base(title) { }

        protected override TagBuilder CreateEditor(object item, object value)
        {
            TagBuilder container = new TagBuilder("span");
            TagBuilder input = new TagBuilder("input");

            var name = GetNameForProperty();

            string objValue = null;

            if (item != null)
            {
                var empty = new DateTime();
                if (!empty.Equals(value))
                {
                    objValue = value != null ? Convert.ToString(value) : null;
                }
                else
                {
                    objValue = null;
                }
            }

            input.Attributes.Add("type", "textbox");
            input.Attributes.Add("name", PropertyName);
            input.Attributes.Add("id", PropertyId);

            if (value != null)
            {
                input.Attributes.Add("value", objValue);
            }

            container.InnerHtml += input.ToString();
            var script =  @"<script type='text/javascript'> $(function() {$( '#" 
                + PropertyId + "' ).datepicker({ dateFormat: '" 
                + (DateFormat ?? dateFormat) +
                "', showOn: 'both', buttonImageOnly: 'true'});});</script>";

            container.InnerHtml += script;
            return container;
        }

        protected override object ExtractValue(NameValueCollection form)
        {
            var name = GetNameForProperty();
            var value = form[name];

            if (value != null)
            {
                DateTime dateTime;
                if (DateTime.TryParse(value, out dateTime))
                {
                    return dateTime;
                }
            }

            return null;
        }

        protected override bool OnValidation(out string error, object value)
        {
            if (IsRequired && value == null)
            {
                error = RequiredText ?? requiredText;
                return false;
            }

            if (value is DateTime)
            {
                if (IsRequired && ((DateTime)value) == new DateTime())
                {
                    error = RequiredText ?? requiredText;
                    return false;
                }

                error = null;

                return true;
            }
            else
            {
                error = "Conversion error.";
                return false;
            }
        }
    }
}
