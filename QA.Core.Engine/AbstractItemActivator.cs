using System;
using System.Linq;
using QA.Core.Engine.UI;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Создание сущностей AbstractItem
    /// </summary>
    public class AbstractItemActivator
    {
        private ITypeFinder _finder;
        private IDefinitionManager _manager;
        private readonly object _syncRoot = new object();

        public AbstractItemActivator(ITypeFinder finder, IDefinitionManager manager)
        {
            _finder = finder;
            _manager = manager;
        }

        /// <summary>
        /// Создать по дискриминатору
        /// </summary>
        /// <param name="discriminator"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        public AbstractItem CreateInstance(string discriminator, bool throwOnError = true)
        {
            var definition = _manager.FindDefinition(discriminator);
            if (throwOnError)
            {
                Throws.IfNot(definition != null, string.Format("Item definition '{0}' is not found", discriminator));
            }

            if (definition == null)
            {
                return null;
            }

            //var type = definition == null ? typeof(AbstractItem) : definition.ItemType;

            var type = definition.ItemType;
            var item = (AbstractItem)Activator.CreateInstance(type, new object[] { });
            return item;
        }
    }
}
