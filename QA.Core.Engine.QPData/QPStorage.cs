using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.Data;
using QA.Core.Data.Repository;

namespace QA.Core.Engine.QPData
{
    /// <summary>
    /// Провайдер доступа к данным структуры сайта.
    /// Должен быть RequestLocal'T
    /// </summary>
    public class QPStorage : IEngineRepository<int, AbstractItem>
    {
        protected AbstractItemModel<int, AbstractItem> Model;
        protected AbstractItemLoader Loader;

        protected QPContext Context
        {
            get { return LinqHelper.Context; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="resolver"></param>
        public QPStorage(AbstractItemLoader loader)
        {
            Model = new AbstractItemModel<int, AbstractItem>();
            Loader = loader;

        }

        public virtual AbstractItem GetById(int id)
        {
            AbstractItem result = null;

            // модель еще не была инициализирована
            // во время обработки текущего запроса
            if (Model.Root == null)
            {
                // TODO: проинициализировать модель всей структурой сайта для данных региона и культуры
                // use service for this

                ReloadAll();
            }

            if (!Model.Items.TryGetValue(id, out result))
            {
                return null;
            }

            return result;
        }

        public virtual AbstractItem[] GetByType<T>() where T : AbstractItem
        {
            // модель еще не была инициализирована
            // во время обработки текущего запроса
            if (Model.Root == null)
            {
                // TODO: проинициализировать модель всей структурой сайта для данных региона и культуры
                // use service for this

                ReloadAll();
            }

            if (!Model.Items.Values
                .Any(a => a is T))
            {
                return null;
            }

            return Model.Items.Values
                .Where(w => w is T)
                .ToArray();
        }

        public virtual void ReloadAll()
        {
            lock (Loader)
            {
                if (Model.Root == null)
                {
                    Loader.LoadAll(Model);
                }

                OnAfterReload();
            }
        }

        protected virtual void OnAfterReload()
        {

        }

        public AbstractItem Get(int id)
        {
            return GetById(id);
        }

        public AbstractItem Load(int id)
        {
            return GetById(id);
        }

        public void Delete(AbstractItem entity)
        {
            throw new NotImplementedException();
        }

        public void Save(AbstractItem entity, bool forcePublish)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(AbstractItem entity, bool forcePublish)
        {
            var affectedItem = Context.QPAbstractItems.FirstOrDefault(x => x.Id == entity.Id);

            Throws.IfArgumentNot(affectedItem != null, _ => affectedItem);

            affectedItem.Parent_ID = entity.ParentId;
            affectedItem.ZoneName = entity.ZoneName;

        }

        public void SaveOrUpdate(AbstractItem entity)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            return Model.ItemsInternal.Any();
        }

        public long Count()
        {
            return Model.ItemsInternal.Count;
        }

        public virtual void Flush()
        {
            if (Context != null)
            {
                Context.SubmitChanges();
            }
        }

        public ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            Model.Dispose();
        }

        ~QPStorage()
        {
            Dispose();
        }
    }
}
