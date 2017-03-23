using System;
using System.Collections.Generic;

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Базовый атрибут для описания типа страницы или виджета
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class DefinitionAttribute : Attribute, IDefinitionModifier
    {
        private readonly string _title;

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Уникальный код типа страницы
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// Иконка
        /// </summary>
        public string IconUrl { get; set; }

		/// <summary>
		/// Надо ли загружать списки идентификаторов для связей many-to-many
		/// </summary>
		public Boolean NeedLoadM2MRelationsIds
		{
			get;
			set;
		}
		
		public string Category
		{
			get;
			set;
		}

        public DefinitionAttribute(string title)
        {
            _title = title;
        }

        public virtual void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            definition.Title = _title;
            definition.Description = Description;
            definition.Discriminator = Discriminator;
            definition.IconUrl = IconUrl;
            definition.Category = Category;
			definition.NeedLoadM2MRelationsIds = NeedLoadM2MRelationsIds;
        }
    }
}
