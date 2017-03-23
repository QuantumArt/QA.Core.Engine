using System.Collections.Generic;
using QA.Core.Collections;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Модель, хранящая DTO структуры сайта.
    /// Должна быть RequestLocal
    /// </summary>
    public class AbstractItemModel<TKey, TNode>
    {
        private ReadOnlyDictionary<TKey, TNode> _readOnlyItems;
        private Dictionary<TKey, TNode> _items;

        internal readonly object Locker = new object();
        internal Dictionary<TKey, TNode> ItemsInternal
        {
            get { return _items; }
        }

        public ReadOnlyDictionary<TKey, TNode> Items { get { return _readOnlyItems; } }
        public TNode Root { get; internal set; }

        public AbstractItemModel()
        {
            _items = new Dictionary<TKey, TNode>();
            _readOnlyItems = new ReadOnlyDictionary<TKey, TNode>(_items);

        }
    }
}
