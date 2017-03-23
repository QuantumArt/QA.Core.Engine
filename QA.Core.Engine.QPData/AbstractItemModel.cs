using System;
using System.Collections.Generic;
using QA.Core.Collections;
using QA.Core.Engine.Interface;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Модель, хранящая DTO структуры сайта.
    /// Должна быть RequestLocal
    /// </summary>
    public class AbstractItemModel<TKey, TNode> : IDisposable
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

        public void Dispose()
        {
            foreach(var item in _items.Values)
            {
                var disposable = item as IDestroyable;
                if(disposable != null)
                {
                    disposable.Destroy();
                }

                //_items.Clear();
                Root = default(TNode);
            }
        }
    }
}
