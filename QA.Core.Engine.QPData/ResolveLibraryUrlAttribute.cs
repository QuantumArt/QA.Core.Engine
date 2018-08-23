using System;
using QA.Core.Engine.QPData;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Формировать полный адрес для поля-ссылки на файл в QP library
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed public class ResolveLibraryUrlAttribute : Attribute, ILoaderOption
    {
        private readonly object _sync = new object();
        private Type _type;
        private string _baseUrl;
        private string _propertyName;

        /// <summary>
        /// Имя поля (Имя поля в QP должно совпадать c PropertyInfo.PropertyName)
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Тип
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        public int FieldId { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fieldId">id поля в qp</param>
        public ResolveLibraryUrlAttribute(int fieldId)
        {
            FieldId = fieldId;
        }

        #region ILoaderOption Members

        void ILoaderOption.AttachTo(Type type, string propertyName)
        {
            _type = type;
            _propertyName = propertyName;
        }

        string ILoaderOption.Process(Quantumart.QPublishing.Database.DBConnector cnn, QPContext ctx, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (_baseUrl == null)
                {
                    lock (_sync)
                    {
                        _baseUrl = cnn.GetUrlForFileAttribute(FieldId, true, ctx.ShouldRemoveSchema);
                    }
                }

                value = _baseUrl + "/" + value;
            }

            return value;
        }

        #endregion

    }
}
