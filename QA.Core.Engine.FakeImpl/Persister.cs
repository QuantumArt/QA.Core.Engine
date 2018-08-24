using System;
#pragma warning disable 1591

namespace QA.Core.Engine.Data
{
    [Obsolete("Use Persister instead")]
    public class FakePersister : Persister
    {
        public FakePersister(IEngineRepository<int, AbstractItem> repository) : base(repository)
        {
        }
    }

    public class Persister : IPersister
    {
        private IEngineRepository<int, AbstractItem> _repositrory;
        public IEngineRepository<int, AbstractItem> Repository
        {
            get { return _repositrory; }
        }

        public Persister(IEngineRepository<int, AbstractItem> repository)
        {
            _repositrory = repository;
        }

        public AbstractItem Get(int id)
        {
            var item = Repository.Get(id);

            return item;
        }

        public AbstractItem[] Get<T>() where T: AbstractItem
        {
            var items = Repository.GetByType<T>();

            return items;
        }

        public T Get<T>(int id) where T : AbstractItem
        {
            return (T)Repository.Get(id);
        }

        public void Save(AbstractItem unsavedItem, bool forcePublish)
        {
            Repository.Update(unsavedItem, forcePublish);
        }

        public void Delete(AbstractItem itemNoMore)
        {
            Repository.Delete(itemNoMore);
        }

        public AbstractItem Copy(AbstractItem source, AbstractItem destination)
        {
            throw new NotImplementedException();
        }

        public AbstractItem Copy(AbstractItem source, AbstractItem destination, bool includeChildren)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Предполагается, что объекты могут как принадлежать так и не принадлежать к текущему контексту
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void Move(AbstractItem source, AbstractItem destination)
        {
            var loadedSource = _repositrory.Get(source.Id);
            var loadedDest = _repositrory.Get(destination.Id);

            if (loadedSource.Parent != null)
            {
                loadedSource.Parent.Children.Remove(loadedSource.Parent);
            }

            loadedSource.Parent = loadedDest;
            destination.Children.Add(loadedSource);

            _repositrory.Update(loadedSource, false);
        }

        public virtual void Flush()
        {
            //Repository.Flush();
        }

        public void Dispose()
        {
            //Repository.Dispose();
        }
    }
}
