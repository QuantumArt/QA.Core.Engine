using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Interface;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class VersioningFilterHierarchyTests
    {
        static int _msk = 4;
        static int _spb = 11;
        static int _rf = 1;
        static int _mo = 3;
        static int _leno = 10;
        static int _kazan = 21;

        private static Region[] GetRegions()
        {
            return new Region[]
                            {
                    new Region { Id = _rf, Title = "russia" },
                        new Region { Id = 2, Parent = 1, Title = "centr" },
                            new Region { Id = _mo, Parent = 2, Alias = "m-obl" },
                                new Region { Id = _msk, Parent = 3, Alias = "msk" },
                                new Region { Id = 5, Parent = 3, Alias = "balashiha" },
                            new Region { Id = _leno, Parent = 2, Alias = "len-obl" },
                                new Region { Id = _spb, Parent = 10, Alias = "spb" },
                                new Region { Id = 12, Parent = 10, Alias = "gatchina" },
                             new Region { Id = 20, Parent = 2, Alias = "tatarstan" },
                                new Region { Id = _kazan, Parent = 20, Alias = "kazan" },


                            };
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Same_Culture_No_Culture_Chosen()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("msk", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null, _spb),
                CreateItem(2, regions, "all1", "all", null, _msk),
                CreateItem(3, regions, "all1", "all", null, _mo),
                CreateItem(4, regions, "all1", "all", null, _leno),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0].Id);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Using_Hierarchy_No_Culture_Chosen()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("msk", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null, _spb),
                CreateItem(3, regions, "all1", "all", null, _mo),
                CreateItem(4, regions, "all1", "all", null, _leno),
                CreateItem(5, regions, "all1", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(3, result[0].Id);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Using_Hierarchy_An_Item_With_Culture_Chosen_Has_Priority()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("msk", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null, _spb),
                CreateItem(3, regions, "all1", "all", null, _msk),
                CreateItem(4, regions, "all1", "all", "ru-ru", _mo),
                CreateItem(5, regions, "all1", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(4, result[0].Id);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Flat_An_Item_With_Culture_Chosen_Has_Priority()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("msk", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null, _spb),
                CreateItem(3, regions, "all1", "all", null, _msk),
                CreateItem(4, regions, "all1", "all", "ru-ru", _msk),
                CreateItem(5, regions, "all1", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(4, result[0].Id);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Ignore_ChildrenRegions()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("m-obl", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null, _spb),
                CreateItem(3, regions, "all1", "all", null, _msk),
                CreateItem(4, regions, "all1", "all", null,  _mo),
                CreateItem(5, regions, "all1", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(4, result[0].Id);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Ignore_Items_With_Empty_Regions_When_REgion_Is_Specified()
        {
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter("msk", null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null),
                CreateItem(3, regions, "all1", "all", null),
                CreateItem(4, regions, "all2", "all", null),
                CreateItem(5, regions, "all3", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(0, result.Length);
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Do_not_Skip_Items_With_Empty_Regions_When_REgion_Is_Not_Specified()
        {
            // елементы без регионов не проходят фильтр, при условии, что текущий регион определен
            // если регион не определен, то элементы проходят
            var regions = GetRegions();
            var provider = new StubRegionHierarchyProvider(regions);

            VersioningFilter filter = new VersioningFilter(null, null, false, useRegionsHierarchy: true, hierarchyProvider: provider);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, regions, "all1", "all", null),
                CreateItem(3, regions, "all1", "all", null),
                CreateItem(4, regions, "all2", "all", null),
                CreateItem(5, regions, "all2", "all", null),
                CreateItem(6, regions, "all3", "all", null),
            };

            var result = filter.Pipe(items).OrderBy(x => x.Id).ToArray();

            Assert.AreEqual(3, result.Length);
        }



        private class StubRegionHierarchyProvider : IRegionHierarchyProvider
        {
            private readonly Region[] _regions;

            public StubRegionHierarchyProvider(params Region[] regions)
            {
                _regions = regions;
            }

            public HierarchyRegion[] GetParentRegionsAndSelf(string alias)
            {
                var region = _regions.FirstOrDefault(x => x.Alias == alias);

                List<HierarchyRegion> result = new List<HierarchyRegion>();
                int level = 0;
                while (region != null)
                {
                    result.Add(new HierarchyRegion(region, level));
                    region = _regions.FirstOrDefault(x => region.Parent.HasValue && x.Id == region.Parent);
                    level++;
                }

                var max = result.Any() ? result.Max(x => level) : 100;

                return result
                    .Select(x => new HierarchyRegion(x, max - x.Level))
                    .ToArray();
            }
        }

        private static AbstractItem CreateItem(int id, Region[] regions, string name, string title, string culture, params int[] regionIds)
        {
            var item = new AbstractItem();

            item.Id = id;
            item.Name = name;
            item.Title = title;

            if (!string.IsNullOrEmpty(culture))
                item.Culture = new Culture() { Key = culture };

            item.Regions.AddRange(regions.Where(x => regionIds.Contains(x.Id)));

            return item;
        }
    }
}
