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
        /// Наименование
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}
