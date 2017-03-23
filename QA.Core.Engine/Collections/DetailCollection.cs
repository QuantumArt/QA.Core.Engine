using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Engine.Collections
{
    public class DetailCollection
    {
        Dictionary<string, DetailCollection.InnerItem<int, object>> _innerDictionary;

        public DetailCollection()
        {
            _innerDictionary = new Dictionary<string, InnerItem<int, object>>();
        }

        public void Add(string key, object value)
        {
            _innerDictionary.Add(key, new InnerItem<int, object>(value));
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public bool Remove(string key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            DetailCollection.InnerItem<int, object> innerValue = null;

            if (_innerDictionary.TryGetValue(key, out innerValue))
            {
                value = innerValue.Value;

                return true;
            }

            value = null;
            return false;
        }

        public ICollection<object> Values
        {
            get { return _innerDictionary.Values.Select(x => x.Value).ToList(); }
        }

        public object this[string key]
        {
            get
            {
                return _innerDictionary[key].Value;
            }
            set
            {
                _innerDictionary[key] = new InnerItem<int, object>(value);
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _innerDictionary.Add(item.Key, new InnerItem<int, object>(item));
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ContainsKey(item.Key);
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Для отслеживания изменений
        /// </summary>
        internal void Add(string key, int id, Type type, object item)
        {
            _innerDictionary.Add(key, new InnerItem<int, object>(item, id, type));
        }

        internal void SetDetail(string key, int id, Type type, object item)
        {
            _innerDictionary[key] = new InnerItem<int, object>(item, id, type);
        }

        internal bool TryInternalGetValue(string key, out DetailCollection.InnerItem<int, object> value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        internal ICollection<KeyValuePair<string, InnerItem<int, object>>> InternalPairs
        {
            get { return _innerDictionary; }
        }
        internal class InnerItem<TId, TValue> : IEquatable<InnerItem<TId, TValue>>
        {
            public TId Id { get; set; }
            public TValue Value { get; set; }
            public Type Type { get; set; }

            public InnerItem(TValue value) : this(value, default(TId), null) { }

            public InnerItem(TValue value, TId id) : this(value, id, null) { }

            public InnerItem(TValue item, TId id, Type type)
            {
                Value = item;
                Id = id;
                Type = type;
            }

            public bool Equals(InnerItem<TId, TValue> other)
            {
                if (other == null || Value == null || other.Value == null)
                {
                    return false;
                }

                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }

                if (object.ReferenceEquals(Value, other.Value))
                {
                    return true;
                }

                return Value.Equals(other.Value);
            }

            public override int GetHashCode()
            {
                if (Value != null)
                {
                    return Value.GetHashCode();
                }
                else
                {
                    return base.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is InnerItem<TId, TValue>)
                {
                    return ((IEquatable<InnerItem<TId, TValue>>)this)
                        .Equals((InnerItem<TId, TValue>)obj);
                }

                return base.Equals(obj);
            }

        }
    }
}
