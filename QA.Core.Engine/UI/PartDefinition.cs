using System;
using System.Collections.Generic;

namespace QA.Core.Engine.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PartDefinitionAttribute : DefinitionAttribute
    {
        public PartDefinitionAttribute(string title)
            : this(title, null) { }

        public PartDefinitionAttribute(string title, string discriminator)
            : base(title)
        {
            Discriminator = discriminator;
        }

        public override void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            definition.IsPage = false;

            base.Modify(definition, all);
        }
    }
}
