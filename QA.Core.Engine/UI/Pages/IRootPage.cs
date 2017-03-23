
using QA.Core.Engine.UI.Pages;
namespace QA.Core.Engine
{
    /// <summary>
    /// Маркер-интерфейс для корневой страницы
    /// </summary>
    public interface IRootPage : IPage
    {
        int DefaultStartPageId { get; }
    }
}
