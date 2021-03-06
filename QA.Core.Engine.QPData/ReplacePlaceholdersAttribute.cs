﻿using System;
using QA.Core.Engine.QPData;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Формировать полный адрес для поля-ссылки на файл в QP library
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed public class ReplacePlaceholdersAttribute : Attribute, ILoaderOption
    {
        private Type _type;
        private string _propertyName;

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public Type Type
        {
            get { return _type; }
        }

        #region ILoaderOption Members

        void ILoaderOption.AttachTo(Type type, string propertyName)
        {
            _type = type;
            _propertyName = propertyName;
        }

        string ILoaderOption.Process(Quantumart.QPublishing.Database.DBConnector cnn, QPContext ctx, string value)
        {
            return value;
        }

        #endregion
    }
}
