using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class VersioningFilterTests
    {
        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Of_Flat_List_No_Versions_When_Culture_Chosen()
        {
            VersioningFilter russian_all_regions = new VersioningFilter(null, "ru-ru", false);
            VersioningFilter english_all_regions = new VersioningFilter(null, "en-us", false);

            VersioningFilter russian_msk = new VersioningFilter("msk", "ru-ru", false);
            VersioningFilter english_msk = new VersioningFilter("msk", "en-us", false);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, "all1", "all", null),
                CreateItem(2, "all2", "all", "en-us"),
                CreateItem(3, "all3", "all", "ru-ru", "msk"),
                CreateItem(4, "all4", "all", "ru-ru", "msk", "spb"),
                CreateItem(5, "all5", "all", "en-us", "msk")
            };

            var result1 = russian_all_regions.Pipe(items).ToList();
            var result2 = english_all_regions.Pipe(items).ToList();
            var result3 = russian_msk.Pipe(items).ToList();
            var result4 = english_msk.Pipe(items).ToList();

            Assert.AreEqual(3, result1.Count, "russian_all_regions");

            Assert.IsTrue(result1.Any(x => x.Id == 1));
            Assert.IsTrue(result1.Any(x => x.Id == 3));
            Assert.IsTrue(result1.Any(x => x.Id == 4));


            Assert.AreEqual(3, result2.Count, "english_all_regions");

            Assert.AreEqual(3, result3.Count, "russian_msk");

            Assert.AreEqual(3, result4.Count, "english_msk");
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Of_Flat_List_Lang_Struct_Versions_When_Culture_Chosen()
        {
            VersioningFilter russian_all_regions = new VersioningFilter(null, "ru-ru", false);
            VersioningFilter english_all_regions = new VersioningFilter(null, "en-us", false);

            VersioningFilter russian_msk = new VersioningFilter("msk", "ru-ru", false);
            VersioningFilter english_msk = new VersioningFilter("msk", "en-us", false);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, "all1", "all", null),
                CreateItem(2, "all1", "all", "ru-ru"),
            };

            var result1 = russian_all_regions.Pipe(items).ToList();
            var result2 = english_all_regions.Pipe(items).ToList();
            var result3 = russian_msk.Pipe(items).ToList();
            var result4 = english_msk.Pipe(items).ToList();

            Assert.AreEqual(1, result1.Count, "russian_all_regions");
            Assert.IsTrue(result1.Any(x => x.Id == 2));

            Assert.AreEqual(1, result2.Count, "english_all_regions");
            Assert.IsTrue(result2.Any(x => x.Id == 1));

            Assert.AreEqual(1, result3.Count, "russian_msk");
            Assert.AreEqual(1, result4.Count, "english_msk");
        }

        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Of_Flat_List_Structural_Versions_When_Culture_Chosen()
        {
            VersioningFilter russian_all_regions = new VersioningFilter(null, "ru-ru", false);
            VersioningFilter english_all_regions = new VersioningFilter(null, "en-us", false);

            VersioningFilter russian_msk = new VersioningFilter("msk", "ru-ru", false);
            VersioningFilter english_msk = new VersioningFilter("msk", "en-us", false);

            VersioningFilter franch_msk = new VersioningFilter("msk", "fr-fr", false);
            VersioningFilter franch_all = new VersioningFilter(null, "fr-fr", false);

            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(1, "all", "all", null),
                CreateItem(2, "all", "all", "en-us"),
                CreateItem(3, "all", "all", "ru-ru"),
                CreateItem(4, "all", "all", "ru-ru", "msk", "spb"),
                CreateItem(5, "all", "all", "ru-ru", "kng"),
                CreateItem(6, "all", "all", "en-us", "msk"),
                // доплнительно.
                CreateItem(7, "all1", "all1", null),
                CreateItem(8, "all1", "all1", "ru-ru"),                
                // доплнительно.
                CreateItem(9, "all1", "all1", "en-us"),    
            };

            var result1 = russian_all_regions.Pipe(items).ToList();
            var result2 = english_all_regions.Pipe(items).ToList();
            var result3 = russian_msk.Pipe(items).ToList();
            var result4 = english_msk.Pipe(items).ToList();
            var result5 = franch_msk.Pipe(items).ToList();
            var result6 = franch_all.Pipe(items).ToList();

            Assert.AreEqual(2, result1.Count, "russian_all_regions");
            Assert.IsTrue(result1.Any(x => x.Id == 3), "russian_all_regions");
            Assert.IsTrue(result1.Any(x => x.Id == 8), "russian_all_regions");

            Assert.AreEqual(2, result2.Count, "english_all_regions");
            Assert.IsTrue(result2.Any(x => x.Id == 2), "english_all_regions");
            Assert.IsTrue(result2.Any(x => x.Id == 9), "english_all_regions");

            Assert.AreEqual(2, result3.Count, "russian_msk");
            Assert.IsTrue(result3.Any(x => x.Id == 4), "russian_msk");
            Assert.IsTrue(result3.Any(x => x.Id == 8), "russian_msk");

            Assert.AreEqual(2, result4.Count, "english_msk");
            Assert.IsTrue(result4.Any(x => x.Id == 6), "english_msk");
            Assert.IsTrue(result4.Any(x => x.Id == 9), "english_msk");

            Assert.AreEqual(2, result5.Count, "franch_msk");
            Assert.IsTrue(result5.Any(x => x.Id == 1), "franch_msk");
            Assert.IsTrue(result5.Any(x => x.Id == 7), "franch_msk");

            Assert.AreEqual(2, result6.Count, "franch_all");
            Assert.IsTrue(result6.Any(x => x.Id == 1), "franch_all");
            Assert.IsTrue(result6.Any(x => x.Id == 7), "franch_all");

        }



        [TestCategory("Versioning")]
        [TestMethod]
        public void Test_Filtering_Of_Flat_List_Structural_Versions_When_Culture_Chosen2()
        {
            VersioningFilter russian_all_regions = new VersioningFilter(null, "ru-ru", false);
            
            List<AbstractItem> items = new List<AbstractItem>()
            {
                CreateItem(4, "all", "all", "ru-ru", "msk", "spb"),
                CreateItem(5, "all", "all", "ru-ru", "kng"),            
            };

            var result1 = russian_all_regions.Pipe(items).ToList();
           
            Assert.AreEqual(0, result1.Count, "russian_all_regions");
        }

        private static AbstractItem CreateItem(int id, string name, string title, string culture, params string[] regions)
        {
            var item = new AbstractItem();

            item.Id = id;
            item.Name = name;
            item.Title = title;

            if (!string.IsNullOrEmpty(culture))
                item.Culture = new Culture() { Key = culture };

            item.Regions.AddRange(regions.Select(x => new Region { Alias = x }));

            return item;
        }
    }
}
