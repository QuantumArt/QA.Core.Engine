using System;

namespace QA.Core.Engine
{
    public interface IPersister : IDisposable
    {
        IEngineRepository<int, AbstractItem> Repository { get; }

        /// <summary>Возвращает элемент или null.</summary>
        AbstractItem Get(int id);

        T Get<T>(int id) where T : AbstractItem;

        void Save(AbstractItem unsavedItem, bool forcePublish);

        void Delete(AbstractItem itemNoMore);

        AbstractItem Copy(AbstractItem source, AbstractItem destination);

        AbstractItem Copy(AbstractItem source, AbstractItem destination, bool includeChildren);

        void Move(AbstractItem source, AbstractItem destination);

        void Flush();
        
        // TODO: подумать насчет событий - хороший способ увязать валидационную бизнес-логику, 
        // логику кеширования, 
        // логику отмены кеша
        
        //event EventHandler<EventArgs> ItemSaving;
        //event EventHandler<EventArgs> ItemSaved;
        //event EventHandler<EventArgs> ItemDeleting;
        //event EventHandler<EventArgs> ItemDeleted;
        //event EventHandler<EventArgs> ItemMoving;
        //event EventHandler<EventArgs> ItemMoved;
        //event EventHandler<EventArgs> ItemCopying;
        //event EventHandler<EventArgs> ItemCopied;
        //event EventHandler<EventArgs> ItemLoaded;
    }
}
