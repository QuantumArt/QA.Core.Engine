using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Engine.Details;
#pragma warning disable 1591

namespace QA.Core.Engine.UI
{
    /// <summary>
    /// Запрещает данным типам страниц и виджетов быть дочерним элементом
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DisallowedChildrenAttribute : Attribute, IInheritDefinitionModifier, IOrdered
    {
        public Type[] _types { get; protected set; }

        public DisallowedChildrenAttribute(params Type[] types)
        {
            _types = types;
        }

        public void Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            foreach (var type in _types.Distinct())
            {
                var targetDefinitions = all.Where(x => type.IsAssignableFrom(x.ItemType)).ToList();

                foreach (var targetDefinition in targetDefinitions)
                {
                    if (!definition.DisallowedChildren.Any(x => x.ItemType == targetDefinition.ItemType))
                    {
                        definition.DisallowedChildren.Add(targetDefinition);
                    }
                }
            }
        }

        public int Order
        {
            get { return 90; }
        }
    }
}
