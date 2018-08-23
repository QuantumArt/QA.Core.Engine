using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Engine.Details;
#pragma warning disable 1591

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Ограничивает набор допустимых дочерних типов
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AllowedChildrenAttribute : Attribute, IInheritDefinitionModifier, IOrdered
    {
        public Type[] _types { get; protected set; }

        public AllowedChildrenAttribute(params Type[] types)
        {
            _types = types;
        }

        public void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            foreach (var type in _types.Distinct())
            {
                var targetDefinitions = all.Where(x => type.IsAssignableFrom(x.ItemType)).ToList();

                if (targetDefinitions.Count == 0)
                {
                    definition.DisallowChildren = true;
                }

                foreach (var targetDefinition in targetDefinitions)
                {
                    if (!definition.AllowedChildren.Any(x => x.ItemType == targetDefinition.ItemType))
                    {
                        definition.AllowedChildren.Add(targetDefinition);
                    }
                }
            }
        }

        public int Order
        {
            get { return 100; }
        }
    }
}
