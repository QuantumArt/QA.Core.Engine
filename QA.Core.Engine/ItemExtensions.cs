using System.Collections.Generic;
using System.Linq;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// расширения дл яработы с иерархией
    /// </summary>
    public static class ItemExtensions
    {
        public static void AttachTo(this AbstractItem item, AbstractItem itemTo)
        {
            if (item.Parent != itemTo)
            {
                item.Parent = itemTo;
            }

            if (!itemTo.Children.Contains(item))
            {
                itemTo.Children.Add(item);
            }
        }

        /// <summary>
        /// Присоединение версии
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemTo"></param>
        public static void AttachVersionTo(this AbstractItem item, AbstractItem itemTo)
        {
            if (item.VersionOf != itemTo)
            {
                item.VersionOf = itemTo;
            }

            if (!itemTo.Versions.Contains(item))
            {
                itemTo.Versions.Add(item);
            }

            if (itemTo.Parent != null)
            {
                item.AttachTo(itemTo.Parent);
            }

            item.Name = itemTo.Name;
        }

        public static string GetTrail(this AbstractItem item)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            AbstractItem i = item;
            do
            {
                items.Add(i);
                i = i.Parent;
            } while (i != null);

            items.Reverse();

            return string.Join("/", items.Select(x => x.Id));
        }

        public static AbstractItem GetClosestStructural(this AbstractItem item)
        {
            var page = item.ClosestPage();
            if (!string.IsNullOrEmpty(page.Name) && page.VersionOf == null)
            {
                return page;
            }

            return null;
        }

        /// <summary>
        /// Получение всех дочерних элементов элемента
        /// </summary>
        /// <param name="item">элемент</param>
        /// <returns></returns>
        public static IEnumerable<AbstractItem> GetChildrenRecursive(this AbstractItem item)
        {
            if (item.CheckHasChildren())
            {
                foreach (var child in item.Children)
                {
                    foreach (var ch in GetChildrenRecursive(child))
                    {
                        yield return ch;
                    }

                    yield return child;
                }
            }
        }
    }
}
