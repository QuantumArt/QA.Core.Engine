#pragma warning disable 1591
namespace QA.Core.Engine
{
    /// <summary>
    /// Ссылка
    /// </summary>
    public interface ILink
    {
        string Contents { get; }

        string ToolTip { get; }

        string Target { get; }

        /// <summary>
        /// Адрес ссылки
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Заголовок ссылки
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Описание ссылки
        /// </summary>
        string Description { get; }
    }
}
