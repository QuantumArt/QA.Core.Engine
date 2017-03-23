﻿using System;
using System.Collections.Generic;

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

