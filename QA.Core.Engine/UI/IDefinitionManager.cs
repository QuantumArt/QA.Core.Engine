using System.Collections.Generic;

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Предоставляет информацию о типах страниц и виджетов
    /// </summary>
    public interface IDefinitionManager
    {
        /// <summary>
        /// Предоставляет информацию для конкретного экземпляра
        /// </summary>
        ItemDefinition GetDefinition(AbstractItem item);

        /// <summary>
        /// Возвращает описание типа страницы или виджета по дискриминатору
        /// </summary>
        /// <param name="discriminator"></param>
        /// <returns></returns>
        ItemDefinition FindDefinition(string discriminator);
        
        /// <summary>
        /// Возвращает список всех описаний типов.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ItemDefinition> GetDefinitions();
    }
}
