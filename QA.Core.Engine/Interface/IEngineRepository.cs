using System;
using System.Collections.Generic;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Репозиторий
    /// </summary>
    /// <typeparam name="TKey">Ключ</typeparam>
    /// <typeparam name="TEntity">Сущность</typeparam>
    public interface IEngineRepository<TKey, TEntity> : IDisposable
    {
        TEntity Get(TKey id);

        AbstractItem[] GetByType<T>() where T : AbstractItem;

        TEntity Load(TKey id);

        void Delete(TEntity entity);

        void Save(TEntity entity, bool forcePublish);

        void Update(TEntity entity, bool forcePublish);

        void SaveOrUpdate(TEntity entity);

        bool Exists();

        long Count();

        void Flush();

        ITransaction BeginTransaction();
    }
}

