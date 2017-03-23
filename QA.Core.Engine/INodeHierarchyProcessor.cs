using System.Collections.Generic;

namespace QA.Core.Engine
{
    public interface INodeHierarchyProcessor
    {
        /// <summary>
        /// Фильтрация потенциальных дочерних элементов
        /// </summary>
        /// <param name="node">текущий элемент</param>
        /// <param name="candidates">Кандидаты в дочерние элементы</param>
        void ProcessNodesChildren(AbstractItem node, IEnumerable<AbstractItem> candidates);
        
        /// <summary>
        /// Проверка валидности сохраняемого элемента
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool CheckNode(AbstractItem parentNode, AbstractItem nodeToCheck);
    }
}
