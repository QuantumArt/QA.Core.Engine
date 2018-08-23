
using QA.Core.Engine.UI.Pages;
#pragma warning disable 1591

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
