using System.Runtime.Serialization;
using QA.Core.Engine.UI;

namespace QA.Core.Engine
{
    /// <summary>
    /// Объект передачи типа контента
    /// </summary>
    [DataContract]
    public class DiscriminatorDTO : ItemDefinition
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Идентификатор расширения
        /// </summary>
        [DataMember]
        public int? PreferredContentId { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}
