using System.Configuration;
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
            QPContext.DefaultConnectionString = ConfigurationManager
                .ConnectionStrings[_connectionStringName].ConnectionString;

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
                    _connectionStringName,
                    QPContext.DefaultXmlMappingSource) :
                LinqHelper.Create(
                    _connectionStringName,
                    _siteName,
                    QPContext.DefaultXmlMappingSource);
        }
    }
}
