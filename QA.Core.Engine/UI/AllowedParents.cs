using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Engine.Details;
#pragma warning disable 1591

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Ограничивает набор допустимых родительских типов
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AllowedParentsAttribute : Attribute, IInheritDefinitionModifier, IOrdered
    {
        public Type[] _types { get; protected set; }

        public AllowedParentsAttribute(params Type[] types)
        {
            _types = types;
        }

        public void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            definition.AllowedParents.Clear();
            foreach (var type in _types.Distinct())
            {
                var targetDefinitions = all.Where(x => type.IsAssignableFrom( x.ItemType)).ToList();

                if (targetDefinitions.Count == 0)
                {
                    definition.DisallowParents = true;
                }

                foreach (var targetDefinition in targetDefinitions)
                {
                    if (!definition.AllowedParents.Any(x => x.ItemType == targetDefinition.ItemType))
                    {
                        definition.AllowedParents.Add(targetDefinition);
                    }
                }
            }
        }

        public int Order
        {
            get { return 140; }
        }
    }
}
