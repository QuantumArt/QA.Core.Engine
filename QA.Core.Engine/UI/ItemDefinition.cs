using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QA.Core.Engine.UI
{
    [DataContract]
    public class ItemDefinition
    {
        /// <summary>
        /// Иконка
        /// </summary>
        [DataMember]
        public string IconUrl { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Является ли данный элемент страницей (false - виджет)
        /// </summary>
        [DataMember]
        public bool IsPage { get; set; }

        /// <summary>
        /// Тип страницы или виджета
        /// </summary>
        [DataMember]
        public string Discriminator { get; set; }

        /// <summary>
        /// Тип сущности
        /// </summary>
        public Type ItemType { get; set; }

        [DataMember]
        public List<string> AllowedZoneNames { get; set; }

        [DataMember]
        public List<ItemDefinition> AllowedChildren { get; set; }

        [DataMember]
        public List<ItemDefinition> AllowedParents { get; set; }

        [DataMember]
        public List<ItemDefinition> DisallowedChildren { get; set; }

        [DataMember]
        public List<ItemDefinition> DisallowedParents { get; set; }

        public ItemDefinition()
        {
            AllowedChildren = new List<ItemDefinition>();
            AllowedParents = new List<ItemDefinition>();
            DisallowedChildren = new List<ItemDefinition>();
            DisallowedParents = new List<ItemDefinition>();
            AllowedZoneNames = new List<string>();
			NeedLoadM2MRelationsIds = false;
        }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool DisallowChildren { get; set; }

        [DataMember]
        public bool DisallowParents { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public int? PreferredContentId { get; set; }

        [DataMember]
        public int Id { get; set; }

		/// <summary>
		/// Надо ли загружать списки идентификаторов для связей many-to-many
		/// </summary>
		[DataMember]
		public bool NeedLoadM2MRelationsIds
		{
			get;
			set;
		}
	}
}
