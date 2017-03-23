using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Data;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void Test_WildCardMatcher()
        {
            var matcher = new WildcardMatcher(WildcardMatchingOption.StartsWith, 
                "bee.ru",
                "*.bee.ru",
                "*.stage.bee.ru",
                "stage.bee.ru",
                "stage.*.ru",
                "*"
            );

            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("msc.bee.ru"));
            Assert.AreEqual("stage.bee.ru", matcher.MatchLongest("stage.bee.ru"));
            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("msc.bee.ru"));
            Assert.AreEqual("stage.*.ru", matcher.MatchLongest("stage.123.ru"));
            Assert.AreEqual("stage.*.ru", matcher.MatchLongest("stage.1232344.ru"));
            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("msc.bee.ru"));
            Assert.AreEqual("*", matcher.MatchLongest("ee.ru"));

            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("moskovskaya-obl.bee.ru"));
        }

        [TestMethod]
        public void Test_WildCardMatcher_Issue01_Incorrect()
        {
            var matcher = new WildcardMatcher(WildcardMatchingOption.FullMatch,
                "bee.ru",
                "*.bee.ru",
                "stage.bee.ru"
            );

            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("msc.bee.ru"));
            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("www.bee.ru"));
            Assert.AreEqual(null, matcher.MatchLongest("bee.ru.artq.com"));

        }


        [TestMethod]
        public void Test_WildCardMatcher_Issue02_Incorrect_one_letter()
        {
            var matcher = new WildcardMatcher(WildcardMatchingOption.FullMatch,
                "bee.ru",
                "*.bee.ru",
                "f.bee.ru"
            );

            Assert.AreEqual("*.bee.ru", matcher.MatchLongest("msc.bee.ru"));
            Assert.AreEqual("f.bee.ru", matcher.MatchLongest("f.bee.ru"));
            Assert.AreEqual(null, matcher.MatchLongest("bee.ru.artq.com"));

        }
    }
}
