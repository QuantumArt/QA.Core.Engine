using System.Collections.Generic;

namespace QA.Core.Engine.Collections
{
    public interface INamedList<T> where T : class, INameable
    {
        T FindNamed(string name);
        ICollection<string> Keys { get; }
        ICollection<T> Values { get; }
        T this[string name] { get; set; }
        void Add(string key, T value);
        bool ContainsKey(string key);
        bool Remove(string key);
        bool TryGetValue(string key, out T value);
    }
}
