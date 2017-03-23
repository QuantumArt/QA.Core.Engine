using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Data.Resolvers;
using QA.Core.Engine.Data;
using QA.Core.Engine.QPData;

namespace QA.Core.Engine.Tests.Integration
{
   // [TestClass]
    public class QPStorageTests
    {
        [TestInitialize]
        public void Initialize()
        {
            var ms = new FileXmlMappingResolver("\\Mapping\\QPContext_Stage.map", "\\Mapping\\QPContext_Live.map");
            QPContext.DefaultXmlMappingSource = ms.GetMapping(true);
        }

        //[TestMethod]
        public void Test_That_The_Root_Page_Is_RootPage()
        {
            var model = new AbstractItemModel<int, AbstractItem>();

            LoadAll(model);

            Assert.IsTrue(model.Root != null);
            Assert.IsInstanceOfType(model.Root, typeof(RootPage));
            Assert.IsTrue(model.Root.Children.Count > 0);
        }

        //[TestMethod]
        public void Test_That_The_Root_Has_Children()
        {
            var model = new AbstractItemModel<int, AbstractItem>();

            LoadAll(model);

            Assert.IsTrue(model.Root.Children.Count > 0);
        }

        //[TestMethod]
        public void Test_That_The_Root_Page_s_Children_Are_StartPage()
        {
            var model = new AbstractItemModel<int, AbstractItem>();

            LoadAll(model);

            foreach (var child in model.Root.Children)
            {
                Assert.IsInstanceOfType(child, typeof(StartPage));                
            }
           
            Assert.IsInstanceOfType(model.Items[167734], typeof(RedirectPage));
        }

        //[TestMethod]
        public void Test_That_An_Item_With_Id_1677734_Is_RedirectPage()
        {
            var model = new AbstractItemModel<int, AbstractItem>();

            LoadAll(model);

            Assert.IsInstanceOfType(model.Items[167734], typeof(RedirectPage));
        }

        private static void LoadAll(AbstractItemModel<int, AbstractItem> model)
        {
            var tf = new FakeTypeFinder();
            var loader = new AbstractItemLoader(
				new AbstractItemActivator(tf, new LocalDefinitionManager(tf, null)), null, new LocalDefinitionManager(tf, null));

            loader.LoadAll(model);
        }
    }
}

