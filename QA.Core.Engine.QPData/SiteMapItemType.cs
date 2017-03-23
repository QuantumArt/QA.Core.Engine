using System;
using System.Linq;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Тип раздела струтуры сайта
    /// </summary>
    public enum SiteMapItemType
    {
        /// <summary>
        /// Не указано
        /// </summary>
        None = 0,

        /// <summary>
        /// Раздел
        /// </summary>
        SiteMap = 1,

        /// <summary>
        /// Виджет
        /// </summary>
        Widget = 2,

        /// <summary>
        /// Контентная версия
        /// </summary>
        ContentVersion = 3
    }
}
