using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Поиск допустимых типов страниц и виджетов
    /// </summary>
    public static class DefinitionsHelper
    {
        private static IDefinitionManager Definitions
        {
            get
            {
                return ObjectFactoryBase.Resolve<IDefinitionManager>();
            }
        }

        /// <summary>
        /// Получить все допустимые типы дочерних виджетов, которые могут быть добавлены в зону данного элемента.
        /// Происходит фильтрация с учетом прав пользователя.
        /// <param name="currentItem">страница или виджет</param>
        /// <param name="zoneName">зона</param>
        /// <param name="user">Текущий пользователь</param>
        /// </summary>
        public static IEnumerable<ItemDefinition> GetAllowedDefinitions(AbstractItem currentItem, 
            string zoneName, 
            IPrincipal user)
        {
            var definitions = Definitions.GetDefinitions();

            var itemDef = definitions.First(x => x.ItemType == currentItem.GetType());

            Throws.IfNot(itemDef != null, string.Format("ItemDefinition for{0} is not found", currentItem.GetContentType().FullName));

            return itemDef.AllowedChildren
                .Where(x => !x.IsPage)
                .Where(x => x.AllowedZoneNames.Contains(zoneName) || x.AllowedZoneNames.Count == 0)
                .ToArray();
        }

        /// <summary>
        /// Получить все допустимые типы дочерних страниц или виджетов для данного элемента.
        /// Происходит фильтрация с учетом прав пользователя.
        /// <param name="currentItem">страница или виджет</param>
        /// <param name="user">Текущий пользователь</param>
        /// </summary>
        public static IEnumerable<ItemDefinition> GetAllowedDefinitions(AbstractItem currentItem, 
            IPrincipal user)
        {
            var definitions = Definitions.GetDefinitions();

            var itemDef = definitions.First(x => x.ItemType == currentItem.GetType());

            Throws.IfNot(itemDef != null, string.Format("ItemDefinition for{0} is not found", currentItem.GetContentType().FullName));

            return itemDef.AllowedChildren
                .ToArray();
        }
    }
}
