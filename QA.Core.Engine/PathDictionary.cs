// Owners: Karlov Nikolay

using QA.Core.Engine.Interface;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QA.Core.Engine
{
    public class PathDictionary : SingletonDictionary<Type, IPathFinder[]>
    {
        static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static void DoWithReadLock(Action action)
        {
            _locker.EnterReadLock();
            try
            {

                action();
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public static T DoWithReadLock<T>(Func<T> action)
        {
            _locker.EnterReadLock();
            try
            {

                return action();
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public static void DoWithWriteLock(Action action)
        {
            _locker.EnterWriteLock();
            try
            {

                action();
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
        public static IPathFinder[] GetFinders(Type itemType)
        {
            return DoWithReadLock(() =>
            {
                if (Instance.ContainsKey(itemType))
                    return Instance[itemType];

                return Instance[itemType] = FindFinders(itemType);
            });
        }


        public static void PrependFinder(Type itemType, IPathFinder finder)
        {
            DoWithWriteLock(() =>
            {
                List<IPathFinder> newFinders = new List<IPathFinder>(GetFinders(itemType));
                newFinders.Insert(0, finder);
                Instance[itemType] = newFinders.ToArray();
            });
        }


        public static void AppendFinder(Type itemType, IPathFinder finder)
        {
            DoWithWriteLock(() =>
            {
                List<IPathFinder> newFinders = new List<IPathFinder>(GetFinders(itemType));
                newFinders.Add(finder);
                Instance[itemType] = newFinders.ToArray();
            });
        }


        public static void RemoveFinder(Type itemType, IPathFinder finder)
        {
            DoWithWriteLock(() =>
            {
                List<IPathFinder> newFinders = new List<IPathFinder>(GetFinders(itemType));
                newFinders.Remove(finder);
                Instance[itemType] = newFinders.ToArray();
            });
        }


        static IPathFinder[] FindFinders(Type itemType)
        {
            object[] attributes = itemType.GetCustomAttributes(typeof(IPathFinder), true);
            List<IPathFinder> pathFinders = new List<IPathFinder>(attributes.Length);
            foreach (IPathFinder finder in attributes)
            {
                pathFinders.Add(finder);
            }
            return pathFinders.ToArray();
        }

        public static PathData GetPath(AbstractItem item, string remainingUrl)
        {
            IPathFinder[] finders = PathDictionary.GetFinders(item.GetContentType());

            foreach (IPathFinder finder in finders)
            {
                PathData data = finder.GetPath(item, remainingUrl);
                if (data != null)
                {
                    data.StopItem = item;
                    return data;
                }
            }

            var greedy = item as IGreedyPage;
            if(greedy != null)
            {
                return new PathData(item, "", "Index", remainingUrl);
            }

            return PathData.None(item, remainingUrl);
        }
    }
}
