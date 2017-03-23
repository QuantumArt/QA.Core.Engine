using System.Collections.Generic;

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// устанавливает параметры
    /// </summary>
    public interface IDefinitionModifier
    {
        /// <summary>
        /// Изменяет описание типов страниц и виджетов
        /// </summary>
        /// <param name="definition">Текущее описание</param>
        /// <param name="all">все описания</param>
        void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all);
    }

    /// <summary>
    /// устанавливает параметры, которые применяются для наследованных классов
    /// </summary>
    public interface IInheritDefinitionModifier
    {
        /// <summary>
        /// Изменяет описание типов страниц и виджетов
        /// </summary>
        /// <param name="definition">Текущее описание</param>
        /// <param name="all">все описания</param>
        void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all);
    }
}
