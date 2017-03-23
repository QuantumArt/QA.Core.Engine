using System;
using System.Collections.Generic;

namespace QA.Core.Engine.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PageDefinitionAttribute : DefinitionAttribute
    {      
        public PageDefinitionAttribute(string title)
            : this(title, null) { }

        public PageDefinitionAttribute(string title, string discriminator)
            : base(title)
        {
            Discriminator = discriminator;
        }

        public override void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            definition.IsPage = true;

            base.Modify(definition, all);
        }
    }
}
