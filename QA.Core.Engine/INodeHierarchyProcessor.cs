using System.Collections.Generic;
#pragma warning disable 1591

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
        /// <param name="parentNode"></param>
        /// <param name="nodeToCheck"></param>
        /// <returns></returns>
        bool CheckNode(AbstractItem parentNode, AbstractItem nodeToCheck);
    }
}
