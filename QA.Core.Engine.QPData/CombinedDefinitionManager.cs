using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QA.Core.Engine.Data;
using QA.Core.Engine.UI;
using QA.Core.Logger;
#pragma warning disable 1591

namespace QA.Core.Engine.QPData
{
    public class CombinedDefinitionManager : LocalDefinitionManager
    {
        public CombinedDefinitionManager(ITypeFinder typeFinder, ILogger logger)
            : base(typeFinder, logger)
        {
        }

        protected override IEnumerable<ItemDefinition> PrepareItems()
        {
            var modelBasedList = base.PrepareItems();
            var ctx = LinqHelper.Context;

            var fromDb = ctx.QPDiscriminators
                .Select(x => x)
                .ToList();

            foreach (var item in fromDb)
            {
                var foundItem = modelBasedList.FirstOrDefault(x => item.Name.Equals(x.Discriminator));
                if (foundItem == null)
                {
                    Debug.WriteLine($"Definition with name {item.Name} is not found.");
                    _logger.Info(() => $"Definition with name {item.Name} is not found.");
                    continue;
                }

                foundItem.IconUrl = item.IconUrlUrl;
                foundItem.Title = item.Title;
                foundItem.Description = item.Description ?? foundItem.Description;
                foundItem.PreferredContentId = item.PreferredContentId;
                foundItem.Id = item.Id;
                foundItem.Category = item.CategoryName ?? foundItem.Category;
                yield return foundItem;
            }
        }
    }
}
