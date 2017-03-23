using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Engine.Data.Structure;

namespace QA.Core.Engine.Data
{

    public class FakeStorage : IEngineRepository<int, AbstractItem>
    {
        Dictionary<int, AbstractItem> _dict = new Dictionary<int, AbstractItem>();

        private INodeHierarchyProcessor _processor;

        public FakeStorage(INodeHierarchyProcessor processor)
        {
            _processor = processor;
            SiteStructure.AttachTo(this);
        }

        #region Fake Content
        private void Setup()
        {
            var root = AddItem(CreateAbstractItem(5, true, "root", true, "root_page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp"));
            var child01 = AddItem(CreateAbstractItem(5, true, "child1", true, "child1_page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp"));
            var child02 = AddItem(CreateAbstractItem(3, true, "child2", true, "child2_page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp"));
            var child03 = AddItem(CreateAbstractItem(3, true, "child1-1", true, "child1-1_page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp"));
            var part01 = AddItem(CreateAbstractItem(2, false, "part1", true, "part1_part", "QA.Mvc.WebApp.Models.TextPart, QA.Mvc.WebApp", "Left"));

            var redirect = AddItem(CreateAbstractItem(0, true, "redirect", true, "Страница-перенаправление", "QA.Mvc.WebApp.Models.RedirectPage, QA.Mvc.WebApp"));

            root.Children.Add(child01);
            root.Children.Add(child02);
            root.Children.Add(redirect);
            child01.Children.Add(child03);
            child01.Children.Add(part01);

            child01.Children.Add(AddItem(CreateAbstractItem(1, false, "part0", true, "part0_part", "QA.Mvc.WebApp.Models.TextPart, QA.Mvc.WebApp", "Center")));

            child02.Children.Add(AddItem(CreateAbstractItem(5, true, "child2-1", true, "child2-1-page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp")));
            child02.Children.Add(AddItem(CreateAbstractItem(3, true, "child2-2", true, "child2-2-page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp")));
            child02.Children.Add(AddItem(CreateAbstractItem(3, true, "child2-3", true, "child2-3-page", "QA.Mvc.WebApp.Models.AbstractPage, QA.Mvc.WebApp")));

            child03.Children.Add(AddItem(CreateAbstractItem(2, false, "part2", true, "part2_part", "QA.Mvc.WebApp.Models.TextPart, QA.Mvc.WebApp", "Left")));
            child03.Children.Add(AddItem(CreateAbstractItem(2, false, "part3", true, "part3_part", "QA.Mvc.WebApp.Models.TextPart, QA.Mvc.WebApp", "Left")));

            SetHierarchy(root);
        }

        private static void SetHierarchy(AbstractItem item)
        {
            foreach (var child in item.Children)
            {
                child.Parent = item;
                SetHierarchy(child);
            }
        }

        private AbstractItem CreateAbstractItem(int maxLevel, bool isPage, string name, bool isVisible, string title, string fullName, string zoneName = null)
        {
            Type t = Type.GetType(fullName);
            var item = (AbstractItem)Activator.CreateInstance(t);

            item.Id = _dict.Count + 1;
            item.IsPage = isPage;
            item.Name = name;
            item.IsVisible = isVisible;
            item.Title = title;
            item.ZoneName = zoneName;

            item.Details.Add("Text", SiteStructure.GenerateRandomContent(maxLevel));
            item.Details.Add("RedirectUrl", "Home\\Index");

            return item;
        }

        private AbstractItem AddItem(AbstractItem item)
        {
            _dict.Add(item.Id, item);
            return item;
        }
        #endregion


        public AbstractItem Get(int id)
        {
            var item = _dict[id];
            return item;
        }

        AbstractItem IEngineRepository<int, AbstractItem>.Get(int id)
        {
            return Get(id);
        }

        public IEnumerable<AbstractItem> Find(string propertyName, object value)
        {
            throw new NotImplementedException();
        }

        public AbstractItem Load(int id)
        {
            return Get(id);
        }

        public void Delete(AbstractItem entity)
        {
            if (_dict.ContainsKey(entity.Id))
            {
                _dict.Remove(entity.Id);
            }

            if (entity.Parent != null)
            {
                entity.Parent.Children.Remove(entity);
                entity.Parent = null;

                foreach (var child in entity.Children)
                {
                    Delete(child);
                }
            }
        }

        public void Save(AbstractItem entity, bool forcePublish)
        {
            if (entity.Id == 0)
            {
                entity.Id = _dict.Keys.Max() + 1;
            }

            if (!_dict.ContainsKey(entity.Id))
            {
                _dict.Add(entity.Id, entity);
            }

            if (entity.Parent != null)
            {
                if (!entity.Parent.Children.Contains(entity))
                {
                    entity.Parent.Children.Add(entity);
                }
            }
            // throw new NotImplementedException();
        }

        public void Update(AbstractItem entity, bool forcePublish)
        {
            if (entity.Id == 0)
            {
                entity.Id = _dict.Keys.Max() + 1;
            }

            if (!_dict.ContainsKey(entity.Id))
            {
                _dict.Add(entity.Id, entity);
            }

            if (entity.Parent != null)
            {
                if (!entity.Parent.Children.Contains(entity))
                {
                    entity.Parent.Children.Add(entity);
                }
            }
            // throw new NotImplementedException();
        }

        public void SaveOrUpdate(AbstractItem entity)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            return true;
        }

        public long Count()
        {
            return _dict.Count;
        }

        public void Flush()
        {

        }

        public ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        internal void InitializeStorage()
        {
            if (SiteStructure.IsInitialized)
            {
                foreach (var item in SiteStructure.Items)
                {

                    if (!(item is IStartPage) && !(item is IRootPage))
                    {
                        if (_dict.ContainsKey(item.Id))
                        {
                            item.Id = _dict.Keys.Max() + 1;
                        }
                    }

                    _dict.Add(item.Id, item);
                }
            }
        }
    }
}
