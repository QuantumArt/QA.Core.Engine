using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using QA.Core.Engine.Web;

namespace QA.Core.Engine.Details
{
    public static class EditorBuilder
    {
        public static Url EditUrl = new Url("/cms/managment/editnode/") { };
        public static Url DeleteUrl = new Url("/cms/managment/deletenode/") { };
        public static Url CreateUrl = new Url("/cms/managment/createnode/") { };

        public static TagBuilder BuildEditorForModel(Type modelType, AbstractItem currentItem, ModelStateDictionary modelState)
        {
            if (!typeof(AbstractItem).IsAssignableFrom(modelType))
            {
                throw new InvalidOperationException("Invalid type of the object.");
            }

            TagBuilder form;
            TagBuilder container;
            WriteBegining(modelType, out form, out container);

            Type editorBase = typeof(IDetailEditor);

            var properties = modelType.GetProperties();

            TagBuilder wr1 = new TagBuilder("div");
            wr1.AddCssClass("field-definition");
            wr1.InnerHtml += GetLabel("Title");
            wr1.InnerHtml += string.Format(@"<input type=""textbox"" id=""Title"" name=""Title"" value=""{0}"" />", currentItem != null ? (currentItem.Title ?? string.Empty) : string.Empty);
            wr1.InnerHtml += GetValidation("Title", modelState).ToString();

            container.InnerHtml += wr1.ToString();

            wr1 = new TagBuilder("div");
            wr1.AddCssClass("field-definition");
            wr1.InnerHtml += GetLabel("Name (a part of the url)");
            wr1.InnerHtml += string.Format(@"<input type=""textbox"" id=""Name"" name=""Name"" value=""{0}"" />", currentItem != null ? (currentItem.Name ?? string.Empty) : string.Empty);
            wr1.InnerHtml += GetValidation("Name", modelState).ToString();

            container.InnerHtml += wr1.ToString();
            container.InnerHtml += @"<hr />";

            // write title, name editors
            foreach (var propertyInfo in properties)
            {
                var attrs = propertyInfo.GetCustomAttributes(true);

                var editor = attrs.Where(x => editorBase.IsAssignableFrom(x.GetType()))
                    .Cast<IDetailEditor>()
                    .FirstOrDefault();

                if (editor != null)
                {
                    editor.Attach(currentItem, propertyInfo);
                    var tb = editor.CreateEditor(currentItem);
                    var wrapper = new TagBuilder("div");
                    wrapper.AddCssClass("field-definition");

                    wrapper.InnerHtml += GetLabel(editor.Title).ToString();

                    var fieldDiv = new TagBuilder("div") { InnerHtml = tb.ToString() };

                    fieldDiv.AddCssClass("editor-field");

                    wrapper.InnerHtml += GetDescription(editor.Description).ToString();
                    fieldDiv.InnerHtml += GetValidation(editor.PropertyName, modelState).ToString();
                    wrapper.InnerHtml += fieldDiv.ToString();
                    container.InnerHtml += wrapper.ToString();
                }
            }

            // write hidden input
            container.InnerHtml += string.Format(@"<input type=""hidden"" id=""_ModelType"" name=""_ModelType"" value=""{0}"" />", modelType.AssemblyQualifiedName);
            container.InnerHtml += string.Format(@"<input type=""hidden"" id=""_ModelId"" name=""_ModelId"" value=""{0}"" />", currentItem.Id);

            form.InnerHtml += container.ToString();
            return form;
        }

        public static bool TryBuildObject(NameValueCollection form, ModelStateDictionary modelState, out object item, string discriminator)
        {
            try
            {
                bool isValid = true;
                Type modelType = Type.GetType(form["_ModelType"]);
                int id = Convert.ToInt32(form["_ModelId"]);
                AbstractItem currentItem = null;
                if (!typeof(AbstractItem).IsAssignableFrom(modelType))
                {
                    throw new InvalidOperationException("Invalid type of the object.");
                }

                if (id != 0)
                {
                    //throw new InvalidOperationException("Creating new objects is not supported yet.");
                    currentItem = ObjectFactoryBase.Resolve<IPersister>().Get(id);
                }
                else
                {
                    
                    currentItem = ObjectFactoryBase.Resolve<AbstractItemActivator>().CreateInstance(discriminator);
                    ((IInjectable<IUrlParser>)currentItem).Set(ObjectFactoryBase.Resolve<IUrlParser>());
                }

                var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                Type editorBase = typeof(IDetailEditor);                

                string title = form["Title"];

                currentItem.Title = title;
                if (!string.IsNullOrEmpty(title))
                {

                }
                else
                {
                    isValid &= false;
                    modelState.AddModelError("Title", "Required.");
                }

                string name = form["Name"];

                currentItem.Name = name;
                if (!string.IsNullOrEmpty(name))
                {

                }
                else
                {
                    isValid &= false;
                    modelState.AddModelError("Name", "Required.");
                }

                foreach (var propertyInfo in properties)
                {
                    var attrs = propertyInfo.GetCustomAttributes(true);

                    var editor = attrs.Where(x => editorBase.IsAssignableFrom(x.GetType()))
                        .Cast<IDetailEditor>()
                        .FirstOrDefault();

                    if (editor != null)
                    {
                        editor.Attach(currentItem, propertyInfo);
                        isValid &= editor.TrySetValue(form, modelState);
                    }
                }

                item = currentItem;

                return isValid;
            }
            catch (Exception ex)
            {
                modelState.AddModelError(string.Empty, ex);
                item = null;
                return false;
            }
        }

        private static void WriteBegining(Type modelType, out TagBuilder form, out TagBuilder container)
        {
            form = new TagBuilder("div");
            container = new TagBuilder("div");
        }

        private static TagBuilder GetLabel(string label)
        {
            var tb = new TagBuilder("div");

            tb.AddCssClass("label-field");
            tb.SetInnerText(label);

            return tb;
        }

        private static TagBuilder GetValidation(string name, ModelStateDictionary modelState)
        {
            ModelState current = null;
            var tb = new TagBuilder("span");

            tb.AddCssClass("field-validation");

            tb.Attributes.Add("validation-for", name);

            if (modelState != null && !modelState.IsValid && modelState.TryGetValue(name, out current))
            {
                if (current.Errors.Count > 0)
                {
                    var error = current.Errors.FirstOrDefault();

                    tb.InnerHtml = error.Exception == null ? error.ErrorMessage : error.Exception.Message;

                    return tb;
                }
            }

            tb.AddCssClass("hidden");
            return tb;
        }

        private static TagBuilder GetDescription(string descr)
        {
            var tb = new TagBuilder("div");

            tb.AddCssClass("label-descr");
            tb.SetInnerText(descr);

            return tb;
        }
    }
}
