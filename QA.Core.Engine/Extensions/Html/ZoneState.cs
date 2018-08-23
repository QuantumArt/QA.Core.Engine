#pragma warning disable 1591

namespace QA.Engine.Extensions.Html
{
    /// <summary>
    ///
    /// </summary>
    public enum ZoneState
    {
        Disabled = 0,
        // отображается панель управления
        Visible = 1,
        // редактирование виджетов
        Editing = 2,
        // режим редактирования контекстной информации OnScreen
        OnScreen = 3
    }
}
