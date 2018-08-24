using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
#pragma warning disable 1591

namespace QA.Core.Engine.Collections
{
    public class ItemList<T> : ContentList<T>, IAbstractItemList<T>, IEnumerable<T>, IHierarchicalEnumerable where T : AbstractItem
    {
        #region Constructors

        public ItemList()
        {
        }

        public ItemList(IEnumerable<T> items)
        {
            Inner = new List<T>(items);
        }

        public ItemList(IEnumerable items, params ItemFilter[] filters)
            : this()
        {
            AddRange(items, filters);
        }

        public ItemList(ICollection<T> items, ItemFilter filter)
            : this()
        {
            if (items.Count == 0)
                return;
            AddRange(items, filter);
        }

        #endregion

        #region Methods

        public void AddRange(IEnumerable<T> items, params ItemFilter[] filters)
        {
            foreach (T item in items)
                if (AllMatch(item, filters))
                    Add(item);
        }

        public void AddRange(IEnumerable items, params ItemFilter[] filters)
        {
            foreach (AbstractItem item in items)
                if (AllMatch(item, filters))
                    Add((T)item);
        }

        public void AddRange(IEnumerable<T> items, IEnumerable<ItemFilter> filters)
        {
            foreach (T item in items)
                if (AllMatch(item, filters))
                    Add(item);
        }

        private bool AllMatch(AbstractItem item, IEnumerable<ItemFilter> filters)
        {
            foreach (ItemFilter filter in filters)
                if (!filter.Match(item))
                    return false;
            return true;
        }

        public void Sort()
        {
            List<T> copy = new List<T>(Inner);
            copy.Sort();
            Inner = copy;
        }

        public void Sort(IComparer<T> comparer)
        {
            List<T> copy = new List<T>(Inner);
            copy.Sort(comparer);
            Inner = copy;
        }

        public bool ContainsAny(IEnumerable<T> items)
        {
            foreach (T item in items)
                if (Contains(item))
                    return true;
            return false;
        }

        public ItemList<OtherT> Cast<OtherT>() where OtherT : AbstractItem
        {
            return new ItemList<OtherT>(this, TypeFilter.Of<OtherT>());
        }
        #endregion

        #region IHierarchicalEnumerable Members

        public IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return new ItemHierarchyData((AbstractItem)enumeratedItem);
        }

        #endregion

        #region Nested type: ItemHierarchyData

        private class ItemHierarchyData : IHierarchyData
        {
            private AbstractItem item;

            public ItemHierarchyData(AbstractItem item)
            {
                this.item = item;
            }

            #region IHierarchyData Members

            IHierarchicalEnumerable IHierarchyData.GetChildren()
            {
                return item.GetChildren();
            }

            IHierarchyData IHierarchyData.GetParent()
            {
                return (item.Parent != null)
                        ? new ItemHierarchyData(item.Parent)
                        : null;
            }

            bool IHierarchyData.HasChildren
            {
                get { return item.GetChildren().Count > 0; }
            }

            object IHierarchyData.Item
            {
                get { return item; }
            }

            string IHierarchyData.Path
            {
                get { return item.Url; }
            }

            string IHierarchyData.Type
            {
                get { return item.GetContentType().Name; }
            }

            #endregion
        }

        #endregion

        #region IZonedList<T> Members

        public IQueryable<T> FindParts(string zoneName)
        {
            return Inner.Where(i => i.ZoneName == zoneName).AsQueryable();
        }

        public IQueryable<T> FindNavigatablePages()
        {
            return FindPages().Where(p => new VisibleFilter().Match(p) && new PublishedFilter().Match(p)).AsQueryable();
        }

        public IQueryable<T> FindPages()
        {
            return this.Where(i => i.ZoneName == null).AsQueryable();
        }

        public IQueryable<T> FindParts()
        {
            return this.Where(i => i.ZoneName != null).AsQueryable();
        }

        public IEnumerable<string> FindZoneNames()
        {
            return this.Select(i => i.ZoneName).Distinct().ToList();
        }

        #endregion
    }

    public class ItemList : ItemList<AbstractItem>
    {
        public ItemList()
        {
        }

        public ItemList(IEnumerable<AbstractItem> items)
            : base(items)
        {
        }

        public ItemList(IEnumerable<AbstractItem> items, ItemFilter filter)
            : base(filter.Pipe(items))
        {
        }
    }
}
