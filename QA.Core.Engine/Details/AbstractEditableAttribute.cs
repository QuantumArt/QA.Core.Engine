using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Web.Mvc;

#pragma warning disable 1591

namespace QA.Core.Engine.Details
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class AbstractEditableAttribute : Attribute, IDetailEditor
    {
        public string Title { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public object DefaultValue { get; set; }
        public string RequiredText { get; set; }

        public string PropertyName { get { return GetNameForProperty(); } }
        public string PropertyId { get { return GetIdForProperty(); } }

        protected readonly ModelStateDictionary ModelState;
        protected object Value;
        protected AbstractItem Item;
        protected PropertyInfo ReflectedInfo;
        protected Type PropertyType { get { return ReflectedInfo.PropertyType; } }

        protected const string requiredText = "This field id required.";
        public AbstractEditableAttribute(string title)
        {
            Title = title;
            ModelState = new ModelStateDictionary();
        }

        protected abstract TagBuilder CreateEditor(object item, object value);

        protected abstract object ExtractValue(NameValueCollection form);

        protected bool Validate(ModelStateDictionary modelState, object value)
        {
            string error = null;
            string name = GetNameForProperty();

            if (value == null)
            {
                modelState.AddModelError(name, RequiredText ?? requiredText);
            }

            if (!OnValidation(out error, value))
            {
                modelState.AddModelError(name, error);
            }

            return true;
        }

        protected abstract bool OnValidation(out string error, object value);

        protected string GetUniqueId()
        {
            return this.GetHashCode().ToString();
        }

        protected virtual string GetNameForProperty()
        {
            if (ReflectedInfo == null) throw new ArgumentNullException("ReflectedInfo");

            return ReflectedInfo.Name;
        }

        protected virtual string GetIdForProperty()
        {
            return string.Format("{0}_{1}", this.GetType().Name, PropertyName).Replace(".", "_").Replace(" ", "_");
        }

        #region IDetailEditor

        void IDetailEditor.Attach(AbstractItem item, PropertyInfo memberInfo)
        {
            ReflectedInfo = memberInfo;
            Item = item;
            if (item != null)
            {
                Value = memberInfo.GetValue(item, new object[] { });
            }
        }

        bool IDetailEditor.TrySetValue(NameValueCollection form, ModelStateDictionary modelState)
        {
            var name = GetNameForProperty();

            try
            {
                // все равно выставляенм значения,
                // так как они будут использованы при отображении формы

                var value = ExtractValue(form) ?? DefaultValue;

                Validate(modelState, value);

                Value = value;

                ReflectedInfo.SetValue(Item, value,
                    BindingFlags.SetProperty | BindingFlags.Public, Type.DefaultBinder,
                    null, CultureInfo.CurrentCulture);

            }
            catch (Exception ex)
            {
                modelState.AddModelError(name, ex);
            }

            return true;
        }

        TagBuilder IDetailEditor.CreateEditor(object item)
        {
            return CreateEditor(item, Value);
        }
        #endregion
    }
}
