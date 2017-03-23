using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Data;
using QA.Core.Engine.UI;

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

            var fromDB = ctx.QPDiscriminators
                .Select(x => x)
                .ToList();

            foreach (var item in fromDB)
            {
                var foundItem = modelBasedList.FirstOrDefault(x => item.Name.Equals(x.Discriminator));
                if (foundItem == null)
                {
                    Debug.WriteLine(string.Format("Definition with name {0} is not found.", item.Name));
                    _logger.Info(_ => string.Format("Definition with name {0} is not found.", item.Name));
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
