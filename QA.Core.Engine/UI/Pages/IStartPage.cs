
using QA.Core.Engine.UI.Pages;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Маркер-интерфейс для стартовой страницы
    /// </summary>
    public interface IStartPage : IPage
    {
        /// <summary>
        /// Список dns-биндингов
        /// </summary>
        string[] GetDNSBindings();
        int Id { get; }
    }
}
