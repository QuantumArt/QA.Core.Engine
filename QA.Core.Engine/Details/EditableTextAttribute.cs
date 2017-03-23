using System;

namespace QA.Core.Engine.Details
{
    public abstract class EditableTextAttribute : AbstractEditableAttribute
    {
        const string minLengthFormat = "The length should be {0} or greater.";
        const string maxLengthFormat = "The length should be less than {0}.";

        public string MinLengthFormat { get; set; }
        public string MaxLengthFormat { get; set; }

        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public EditableTextAttribute(string title) : base(title) { }

        protected override bool OnValidation(out string error, object value)
        {
            return OnTextValidation(out error, Convert.ToString(value));
        }

        protected virtual bool OnTextValidation(out string error, string value)
        {
            if (IsRequired && string.IsNullOrEmpty(value))
            {
                error = RequiredText ?? requiredText;
                return false;
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                {
                    error = "The value is empty.";
                    return false;
                }
                else if (MinLength > 0 && value.Length < MinLength)
                {
                    error = string.Format(MinLengthFormat ?? minLengthFormat, MinLength);
                    return false;
                }
                else if (MaxLength > 0 && value.Length > MaxLength)
                {
                    error = string.Format(MaxLengthFormat ?? maxLengthFormat, MaxLength);
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
