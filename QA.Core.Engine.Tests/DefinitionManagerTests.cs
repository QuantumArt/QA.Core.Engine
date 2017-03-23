using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Data;
using QA.Core.Engine.UI;
using QA.Core.Engine.UI.Pages;

namespace QA.Core.Engine.Tests
{

    /*  *HtmlPage True
       * NewsContainer True
       * TextPart 
       * OnlyOnStructuralPagePart True
       * XmlContent True
      *  BannerPart False
     * */
    #region Test Parts

    [PartDefinition("Абстрактный виджет", Discriminator = "TextPart")]
    public class TextPart : AbstractItem, IWebPart { }

    [AllowedParents(typeof(IStructuralPage))]
    [PartDefinition("Виджет для страницы", Discriminator = "OnlyOnStructuralPagePart")]
    public class OnlyOnStructuralPagePart : TextPart { }

    [AllowedParents(typeof(IWebPart))]
    [PartDefinition("Виджет для виджета-контейнера", Discriminator = "OnlyOnPartPart")]
    public class OnlyOnPartPart : TextPart { }

    [AllowedParents(typeof(IStructuralPage))]
    [PartDefinition("BANNER виджет", Discriminator = "BannerPart")]
    public class BannerPart : TextPart { }

    #endregion

    #region Test Pages

    [PageDefinition("Абстрактная страница", Discriminator = "AbstractFakePage")]
    public class AbstractFakePage : AbstractItem, IPage { }

    [PageDefinition("HtmlPage", Discriminator = "HtmlPage")]
    public class HtmlPage : AbstractFakePage, IStructuralPage
    {
        public string Text
        {
            get
            {
                return GetDetail<string>("Text", null);
            }
            set
            {
                SetDetail("Text", value);
            }
        }
    }

    [AllowedParents(typeof(IStructuralPage))]
    [PageDefinition("XmlContent", Discriminator = "XmlContent")]
    public class OnlyIntoStructuralPage : HtmlPage { }

    [AllowedParents(typeof(IStructuralPage))]
    [PageDefinition("RedirectPage", Discriminator = "RedirectPage")]
    public class RedirectPage : AbstractFakePage { }

    [AllowedParents(typeof(IRootPage))]
    [PageDefinition("StartPage", Discriminator = "StartPage")]
    public class StartPage : HtmlPage, IStartPage
    {
        public string Bindings
        {
            get { throw new NotImplementedException(); }
        }

        public string[] GetDNSBindings()
        {
            throw new NotImplementedException();
        }
    }

    [AllowedChildren(typeof(IStartPage))]
    [AllowedParents(typeof(IFakePage))]
    [PageDefinition("RootPage", Discriminator = "RootPage")]
    public class RootPage : AbstractItem, IRootPage
    {
        public int DefaultStartPageId
        {
            get { throw new NotImplementedException(); }
        }
    }

    [AllowedChildren(typeof(NewsItem))]
    [DisallowedChildren(typeof(BannerPart))]
    [PageDefinition("NewsContainer", Discriminator = "NewsContainer")]
    public class NewsContainer : HtmlPage { }

    [AllowedParents(typeof(NewsContainer))]
    [AllowedChildren(typeof(IFakePage))]
    [PageDefinition("NewsItem", Discriminator = "NewsItem")]
    public class NewsItem : AbstractFakePage, IPage { }

    [AllowedParents(typeof(NewsContainer))]
    [AllowedChildren(typeof(IFakePage))]
    [PageDefinition("NewsItemExtended", Discriminator = "NewsItemExtended")]
    public class NewsItemExtended : NewsItem { }

    #endregion

    [TestClass]
    public class DefinitionManagerTests
    {
        [TestMethod]
        public void Test_GetDefinitions_Number_Of_Items_Is_Correct()
        {

            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToArray();//.ToDictionary(d => d.Discriminator);

            Assert.IsNotNull(defs);
            Assert.AreEqual(14, defs.Count(), "Number of definitions is not correct. Expected 14");

            // For debug only
#if DEBUG
            string format = "{0};{1};{2}";
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var def in defs)
            {
                foreach (var item in def.AllowedChildren)
                {
                    var token = string.Empty;
                    if (i < defs.Length)
                    {
                        token = defs[i].Discriminator + ";" + (UInt32)defs[i].Discriminator.GetHashCode();
                        i++;
                    }
                    sb.AppendLine(string.Format(format, (UInt32)item.Discriminator.GetHashCode(), (UInt32)def.Discriminator.GetHashCode(), token));
                }
            }
            var edges = sb.ToString();
            Trace.WriteLine(edges);
#endif
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_Parts_Added_To_All_Types()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);
            foreach (var def in defs.Values)
            {
                Assert.IsTrue(def.AllowedChildren.ContainsDefinitions("TextPart"));
            }
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_OnlyOnStructuralPage_Constraint()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["OnlyOnStructuralPagePart"];

            Assert.AreEqual(4, item.AllowedParents.Count());

            foreach (var parent in item.AllowedParents)
            {
                Assert.IsTrue(parent.IsPage);
            }
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_OnlyOnPartPart_Constraint()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["OnlyOnPartPart"];

            Assert.AreEqual(4, item.AllowedParents.Count());

            foreach (var parent in item.AllowedParents)
            {
                Assert.IsTrue(!parent.IsPage);
            }
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_AllowedChildred_Page_Restrictions()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["RootPage"];
            Assert.AreEqual(0, item.AllowedParents.PagesCount());
            Assert.AreEqual(1, item.AllowedChildren.PagesCount());
            Assert.IsTrue(item.AllowedChildren.ContainsDefinitions("StartPage"));
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_Page_Restrictions_With_Inheritance()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["NewsItem"];
            Assert.AreEqual(0, item.AllowedChildren.PagesCount());
            Assert.AreEqual(1, item.AllowedParents.PagesCount());
            Assert.IsTrue(item.AllowedParents.ContainsDefinitions("NewsContainer"));

            item = defs["NewsItemExtended"];
            Assert.AreEqual(0, item.AllowedChildren.PagesCount());
            Assert.AreEqual(1, item.AllowedParents.PagesCount());
            Assert.IsTrue(item.AllowedParents.ContainsDefinitions("NewsContainer"));
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_Parent_Page_Restrictions_With_Inheritance()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["NewsContainer"];
            Assert.AreEqual(2, item.AllowedChildren.PagesCount());
            Assert.IsTrue(item.AllowedChildren.ContainsDefinitions("NewsItem", "NewsItemExtended"));
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_DisallowedChildren_Is_Not_Presented()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["NewsContainer"];
            Assert.IsTrue(!item.AllowedChildren.ContainsDefinitions("BannerPart"));
        }

        [TestMethod]
        public void Test_GetDefinitions_Check_Disallowed()
        {
            var manager = new LocalDefinitionManager(new FakeTypeFinder(), null);
            var defs = manager.GetDefinitions().ToDictionary(d => d.Discriminator);

            var item = defs["BannerPart"];
            Assert.IsTrue(!item.AllowedParents.ContainsDefinitions("NewsContainer"));
        }
    }

    public static class TestExtensions
    {
        public static int PagesCount(this IEnumerable<ItemDefinition> cont)
        {
            return cont.Count(x => x.IsPage);
        }

        public static int PartsCount(this IEnumerable<ItemDefinition> cont)
        {
            return cont.Count(x => !x.IsPage);
        }

        public static bool ContainsDefinitions(this IEnumerable<ItemDefinition> cont, params string[] discriminators)
        {
            foreach (var item in discriminators)
            {
                if (!cont.Any(x => x.Discriminator == item))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
