﻿using System.Configuration;
using QA.Core.Data;
using QA.Core.Data.Repository;

namespace QA.Core.Engine.QPData.Context
{
    /// <summary>
    /// Контект к БД QP
    /// </summary>
    public partial class QPUnitOfWork : L2SqlUnitOfWorkBase<QPContext>
    {
        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public QPUnitOfWork(string connectionString) :
            base(connectionString, null)
        {
        }

        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="mappingSource">Источник маппинга</param>
        public QPUnitOfWork(string connectionString, IXmlMappingResolver mappingSource):
            base(connectionString, mappingSource)
        {

        }

        /// <summary>
        /// Конструирует объект
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="mappingSource">Источник маппинга</param>
        /// <param name="siteName">Имя сайта</param>
        public QPUnitOfWork(string connectionString, string siteName, IXmlMappingResolver mappingSource) :
            base(connectionString, siteName, mappingSource)
        {
        }

        /// <summary>
        /// Расширение создания объекта
        /// </summary>
        protected override void OnCreated()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[ConnectionString];
            if (connectionStringSettings != null)
            {
                QPContext.DefaultConnectionString = connectionStringSettings.ConnectionString;
            }
            else
            {
                QPContext.DefaultConnectionString = ConnectionString;
            }

            if (!string.IsNullOrEmpty(_siteName))
            {
                QPContext.DefaultSiteName = _siteName;
            }

            if (_mappingSource != null)
            {
                QPContext.DefaultXmlMappingSource = _mappingSource.GetCurrentMapping();
            }
        }

        /// <summary>
        /// Создает контекст
        /// </summary>
        /// <returns></returns>
        protected override QPContext CreateContext()
        {
            return string.IsNullOrEmpty(_siteName) ?
                LinqHelper.Create(
                    ConnectionString,
                    QPContext.DefaultXmlMappingSource) :
                LinqHelper.Create(
                    ConnectionString,
                    _siteName,
                    QPContext.DefaultXmlMappingSource);
        }
    }
}
