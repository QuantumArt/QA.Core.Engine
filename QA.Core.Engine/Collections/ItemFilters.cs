using System;
using System.Linq;
using System.Collections.Generic;

namespace QA.Core.Engine.Collections
{
    public abstract class ItemFilter : IDisposable
    {      
        public abstract bool Match(AbstractItem item);

        public virtual void Filter(IList<AbstractItem> items)
        {
            using (this)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (!Match(items[i]))
                    {
                        items.RemoveAt(i);
                    }
                }
            }
        }
             
        public virtual IEnumerable<T> Pipe<T>(IEnumerable<T> items)
            where T : AbstractItem
        {
            using (this)
            {
                foreach (T item in items)
                {
                    if (Match(item))
                    {
                        yield return item;
                    }
                }
            }
        }

        public virtual IEnumerable<AbstractItem> Pipe(IEnumerable<AbstractItem> items)
        {
            return Pipe<AbstractItem>(items);
        }
     
        public static void Filter(IList<AbstractItem> items, ItemFilter filter)
        {
            filter.Filter(items);
        }

        #region IDisposable Members
                
        public virtual void Dispose()
        {
            // do nothing
        }

        #endregion

        #region implicit operator Func<AbstractItem, bool>
        public static implicit operator Func<AbstractItem, bool>(ItemFilter filter)
        {
            if (filter == null)
                return (ci) => true;
            return filter.Match;
        }

        public static implicit operator ItemFilter(Func<AbstractItem, bool> isPositiveMatch)
        {
            if (isPositiveMatch == null)
                return new NullFilter();
            return new DelegateFilter(isPositiveMatch);
        }
        #endregion

        #region Operators

        public static ItemFilter operator &(ItemFilter f1, ItemFilter f2)
        {
            return new AllFilter(f1, f2);
        }

        public static ItemFilter operator |(ItemFilter f1, ItemFilter f2)
        {
            return new DelegateFilter(i => f1.Match(i) || f2.Match(i));
        }

        public static ItemFilter operator +(ItemFilter f1, ItemFilter f2)
        {
            return new AllFilter(f1, f2);
        }

        public static ItemFilter operator -(ItemFilter f1, ItemFilter f2)
        {
            return new AllFilter(f1, new InverseFilter(f2));
        }


        #endregion
    }

    public class TypeFilter : ItemFilter
    {
        private Type _type;

        public TypeFilter(Type type)
        {
            _type = type;
        }

        public static ItemFilter Of<T>()
        {
            return new TypeFilter(typeof(T));
        }

        public override bool Match(AbstractItem item)
        {
            return item != null && item.GetContentType() == _type;
        }
    }

    public class AccessFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return true;
        }
    }

    public class VisibleFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return true;
        }
    }

    public class PublishedFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return true;
        }
    }

    public class InverseFilter : ItemFilter
    {
        private ItemFilter filterToInverse;

        public InverseFilter(ItemFilter filterToInverse)
        {
            this.filterToInverse = filterToInverse;
        }

        public override bool Match(AbstractItem item)
        {
            return !filterToInverse.Match(item);
        }

        public static void FilterInverse(IList<AbstractItem> items, ItemFilter filterToInverse)
        {
            Filter(items, new InverseFilter(filterToInverse));
        }
    }
    
    public class NullFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return true;
        }
    }

    public class IsPartFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return item.IsPage == false;
        }
    }

    public class IsPageFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return item.IsPage;
        }
    }

    public class DelegateFilter : ItemFilter
    {
        readonly Func<AbstractItem, bool> isPositiveMatch;

        public DelegateFilter(Func<AbstractItem, bool> isPositiveMatch)
        {
            if (isPositiveMatch == null) throw new ArgumentNullException("isPositiveMatch");

            this.isPositiveMatch = isPositiveMatch;
        }

        public override bool Match(AbstractItem item)
        {
            return isPositiveMatch(item);
        }
    }

    public class AllFilter : ItemFilter
    {
        private ItemFilter[] filters;

        public AllFilter(params ItemFilter[] filters)
        {
            this.filters = filters ?? new ItemFilter[0];
        }

        public AllFilter(IEnumerable<ItemFilter> filters)
        {
            this.filters = new List<ItemFilter>(filters).ToArray();
        }
               
        public ItemFilter[] Filters
        {
            get { return filters; }
            set { filters = value; }
        }

        public override bool Match(AbstractItem item)
        {
            foreach (ItemFilter filter in filters)
                if (!filter.Match(item))
                    return false;
            return true;
        }

        public override IEnumerable<T> Pipe<T>(IEnumerable<T> items)
        {
            IEnumerable<T> filtered = items;
            foreach (ItemFilter filter in filters)
            {
                filtered = filter
                    .Pipe(filtered)
                    .ToList();

                if (!filtered.Any())
                    break;
            }

            return filtered;
        }

        public static ItemFilter Wrap(IList<ItemFilter> filters)
        {
            if (filters == null || filters.Count == 0)
                return new NullFilter();
            else if (filters.Count == 1)
                return filters[0];
            else
                return new AllFilter(filters);
        }

        public static ItemFilter Wrap(params ItemFilter[] filters)
        {
            return Wrap((IList<ItemFilter>)filters);
        }
    }
    public class NavigationFilter : ItemFilter
    {
        public override bool Match(AbstractItem item)
        {
            return item != null && item.IsPage;
        }
    }
}
