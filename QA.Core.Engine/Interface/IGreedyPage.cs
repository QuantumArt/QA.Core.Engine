using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Engine.Interface
{
    /// <summary>
    ///  Маркер-интерфейс для страниц, обрабатывающих вложенные адреса
    /// </summary>
    public interface IGreedyPage
    {
        /// <summary>
        /// Применяется алгоритм
        /// </summary>
        bool IsGreedy { get; }
        /// <summary>
        /// Применение алгоритма в случае, если не найдена страница
        /// </summary>
        bool ApplyWhenNotFound { get; }
    }
}
