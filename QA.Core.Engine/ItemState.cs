
using System.ComponentModel;
using QA.Core.Web;

namespace QA.Core.Engine
{
    /// <summary>
    /// Статус страницы или виджета
    /// </summary>
    //[JSUsing("Enums", null)]
    public enum ItemState
    {
        /// <summary>
        /// Создан
        /// </summary>
        [Description("Created")]
        //[JSUsing("ItemState", null)]
        Created,

        /// <summary>
        /// Опубликован
        /// </summary>
        [Description("Published")]
        //[JSUsing("ItemState", null)]
        Published,

        /// <summary>
        /// Подтверждено
        /// </summary>
        [Description("Approved")]
        //[JSUsing("ItemState", null)]
        Approved,

        /// <summary>
        /// Без статуса
        /// </summary>
        [Description("None")]
        //[JSUsing("ItemState", null)]
        None
    }
}
