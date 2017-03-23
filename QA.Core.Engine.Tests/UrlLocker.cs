using System;
using System.Collections.Concurrent;

namespace QA.Core.Engine.Tests
{
    public static class Locker
    {
        static ConcurrentDictionary<Type, object> _lockers = new ConcurrentDictionary<Type, object>();
        public static object ForType<T>()
        {
            return _lockers.GetOrAdd(typeof(T), t => new object());
        }
    }
}
