using System.Data;
using System.Data.Linq.Mapping;
using QA.Core.Data.Repository;
using QA.Core.Web;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Расширение контекста
    /// </summary>
    public partial class QPContext
    {
        /// <summary>
        /// Создает контектс
        /// </summary>
        /// <param name="connection">Экземпляр подклчюения к БД</param>
        /// <param name="mappingSource">Источник маппинга стурктуры</param>
        /// <returns></returns>
        public static QPContext Create(IDbConnection connection, MappingSource mappingSource)
        {
            QPContext ctx = new QPContext(connection, mappingSource);
            ctx.SiteName = QPContext.DefaultSiteName;
            ctx.ConnectionString = connection.ConnectionString;
            return ctx;
        }
    }

    /// <summary>
    /// Расширение хелпера linq
    /// </summary>
    internal static partial class LinqHelper
    {
        /// <summary>
        /// Менеджер подключений к БД
        /// </summary>
        private static RequestLocal<ConnectionManager> _connManager =
            new RequestLocal<ConnectionManager>(() => new ConnectionManager());

        /// <summary>
        /// Создает контекст
        /// </summary>
        /// <param name="connectionName">Имя подключение к БД</param>
        /// <param name="mappingSource">Источник маппинга</param>
        /// <returns></returns>
        public static QPContext Create(string connectionName, MappingSource mappingSource)
        {
            IDbConnection conn = _connManager.Value.GetConnection(connectionName);

            QPContext ctx = QPContext.Create(conn, mappingSource);
            
            if (InternalDataContext == null)
            {
                InternalDataContext = ctx;
            }

            return ctx;
        }

        /// <summary>
        /// Создает контекст
        /// </summary>
        /// <param name="connectionName">Имя подключение к БД</param>
        /// <param name="mappingSource">Источник маппинга</param>
        /// <param name="siteName">Имя сайта</param>
        /// <returns></returns>
        public static QPContext Create(string connectionName, string siteName, MappingSource mappingSource)
        {
            IDbConnection conn = _connManager.Value.GetConnection(connectionName);

            QPContext ctx = QPContext.Create(conn, siteName, mappingSource);

            if (InternalDataContext == null)
            {
                InternalDataContext = ctx;
            }

            return ctx;
        }
    }
}
