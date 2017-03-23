using System;
namespace QA.Core.Engine.Editing
{
    /// <summary>
    /// Получение имя поля QP по названию
    /// </summary>
    public interface IAbstractItemFieldNameResolver
    {
        /// <summary>
        /// Получение имя поля QP по названию
        /// <param name="name">Название поля</param>
        /// <returns>Имя поля</returns>
        /// </summary>
        string Resolve(string name);
    }
}
