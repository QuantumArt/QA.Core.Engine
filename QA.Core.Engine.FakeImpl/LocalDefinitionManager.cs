using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using QA.Core.Engine.Details;
using QA.Core.Engine.UI;

namespace QA.Core.Engine.Data
{
    public class LocalDefinitionManager : IDefinitionManager
    {
        private ITypeFinder _typeFinder;
        private List<ItemDefinition> _definitions = null;
        private object _syncRoot = new object();
        protected readonly ILogger _logger;

        public LocalDefinitionManager(ITypeFinder typeFinder, ILogger logger)
        {
            _typeFinder = typeFinder;
            _logger = logger;
        }

        public ItemDefinition GetDefinition(AbstractItem item)
        {
            CheckInitialized();

            return _definitions.FirstOrDefault(x => x.ItemType == item.GetType());
        }

        private void CheckInitialized()
        {
            if (_definitions == null)
            {
                lock (_syncRoot)
                {
                    if (_definitions == null)
                    {
                        _definitions = new List<ItemDefinition>();
                        _definitions.AddRange(GetItems());

                    }
                }
            }
        }

        public IEnumerable<ItemDefinition> GetDefinitions()
        {
            CheckInitialized();

            return _definitions.ToArray();
        }

        private IEnumerable<ItemDefinition> GetItems()
        {
            var itemDefinitions = PrepareItems().ToArray();

            // TODO учесть наследование при формировании списков

            var children = new Dictionary<ItemDefinition, List<ItemDefinition>>();
            var parents = new Dictionary<ItemDefinition, List<ItemDefinition>>();

            // заносим все ограничения в словари.
            foreach (var itemDefinition in itemDefinitions)
            {
                // allowed children
                var allowed = itemDefinitions
                    .Where(x =>
                        // элементы, у которых нет ограничения на родительские элементы
                        (x.AllowedParents.Count == 0 && !x.DisallowParents)
                            // или элементы с ограниченным списком родителей, и текущий тип наследуется от какого-либо из них
                        || x.AllowedParents.Any(y => y.ItemType.IsAssignableFrom(itemDefinition.ItemType))
                        || (!x.IsPage && itemDefinition.IsPage && x.AllowedParents.Count == 0 && !x.DisallowParents))
                    // при этом если текущий тип - виджет, то в него нельзя добавлять страницы
                    .Where(x => itemDefinition.IsPage || !x.IsPage)
                    .Where(x => !itemDefinition.DisallowedChildren.Contains(x));

                allowed = allowed
                    // фильтруем полученные типы с учетом ограничений самого типа
                    .Where(x =>
                        // если нет ограничения на детей
                        (itemDefinition.AllowedChildren.Count == 0 && !itemDefinition.DisallowChildren)
                            // или есть ограничение, и типы являются наследниками типов из ограничения
                        || itemDefinition.AllowedChildren.Any(y => y.ItemType.IsAssignableFrom(x.ItemType))
                            // для виджетов если не указаны их ограничения
                        || (!x.IsPage && (x.AllowedParents.Any(y => y.ItemType.IsAssignableFrom(itemDefinition.ItemType))
                        || (x.AllowedParents.Count == 0 && !x.DisallowParents))));

                children.Add(itemDefinition, allowed.Distinct().ToList());

            }

            // сверяем ограничения со словарями
            foreach (var key in children.Keys)
            {
                var allowedChildren = children[key];

                var allowedParents = children
                    .Where(x => x.Value.Contains(key))
                    .Select(y => y.Key)
                    .ToList();

                key.AllowedChildren = allowedChildren;
                key.AllowedParents = allowedParents;
            }

            return itemDefinitions;
        }

        protected virtual IEnumerable<ItemDefinition> PrepareItems()
        {
            return PrepareItemsInternal().ToArray();
        }
        protected virtual IEnumerable<ItemDefinition> PrepareItemsInternal()
        {
            var items = _typeFinder.Find(typeof(AbstractItem))
               .Where(x => !x.IsAbstract && x.IsClass && x.IsPublic && x != typeof(AbstractItem));

            var definitions = new List<ItemDefinition>();

            foreach (var item in items)
            {
                var definition = new ItemDefinition { ItemType = item, Title = item.Name };
                definitions.Add(definition);
            }

            foreach (var definition in definitions)
            {
                {
                    var modifiers = definition.ItemType.GetCustomAttributes(typeof(IDefinitionModifier), false);

                    foreach (var modifier in modifiers.Cast<IDefinitionModifier>())
                    {
                        modifier.Modify(definition, definitions.ToList());
                    }
                }
                {
                    var modifiers = definition.ItemType.GetCustomAttributes(typeof(IInheritDefinitionModifier), true).Cast<IInheritDefinitionModifier>();
                    var latest = modifiers.GroupBy(x => x.GetType())
                        .Select(x => new { Key = x.Key, Item = x.FirstOrDefault() })
                        .Select(x => x.Item)
                        .OrderBy(x => (x is IOrdered) ? ((IOrdered)x).Order : int.MaxValue);

                    foreach (var modifier in latest)
                    {
                        modifier.Modify(definition, definitions.ToList());
                    }
                }

                if (!string.IsNullOrEmpty(definition.Discriminator))
                {
                    yield return definition;
                }
                else
                {
                    Trace.WriteLine(string.Format("Discriminator for {0} is not specified", definition.ItemType.FullName));
                }
            }
        }
        public ItemDefinition FindDefinition(string discriminator)
        {
            CheckInitialized();

            return _definitions
                .FirstOrDefault(x => x.Discriminator
                    .Equals(discriminator, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
