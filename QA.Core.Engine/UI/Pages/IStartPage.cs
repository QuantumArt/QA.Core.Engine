
using QA.Core.Engine.UI.Pages;
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
