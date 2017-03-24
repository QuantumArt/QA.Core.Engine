using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.FakeImpl;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class TokenMatcherTests
    {
        [TestMethod]
        public void Test_RegionMatcher_dns_positive()
        {
            lock (Locker.ForType<Url>())
            {
                var config = new CultureUrlParserConfig();

                config.MatchingPatterns.Add(
                    new UrlMatchingPattern
                    {
                        Value = "{region}.bee.ru/{culture}",
                        DefaultCultureToken = "ru-ru"
                    });

                UrlTokenMatcher matcher = new UrlTokenMatcher(config);

                var regions = new string[] { "msc", "spb" };
                var cultures = new string[] { "ru-ru", "en-us" };

                var result = matcher.Match("http://msc.bee.ru/ru-ru/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://msc.bee.ru/qwerty", result.SanitizedUrl.ToString());

                result = matcher.Match("http://msc.bee.ru/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://msc.bee.ru/qwerty", result.SanitizedUrl.ToString());

                result = matcher.Match("http://msc.bee.ru/en-us/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("en-us", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://msc.bee.ru/qwerty", result.SanitizedUrl.ToString());


                result = matcher.Match("/en-us/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("en-us", result.Culture);
                Assert.AreEqual(null, result.Region);
                Assert.AreEqual("/qwerty", result.SanitizedUrl.ToString());
            }
        }
        [TestMethod]
        public void Test_RegionMatcher_segments_positive()
        {
            lock (Locker.ForType<Url>())
            {
                var config = new CultureUrlParserConfig();

                config.MatchingPatterns.Add(
                  new UrlMatchingPattern
                  {
                      Value = "bee.ru/{culture}/{region}",
                      DefaultCultureToken = "ru-ru"
                  });

                config.MatchingPatterns.Add(
                  new UrlMatchingPattern
                  {
                      Value = "bee.ru/{region}",
                      DefaultCultureToken = "ru-ru"
                  });

                //config.MatchingPatterns.Add(
                //    new UrlMatchingPattern
                //    {
                //        Value = "bee.ru/{culture}",
                //        DefaultCultureToken = "ru-ru"
                //    });

                UrlTokenMatcher matcher = new UrlTokenMatcher(config);

                var regions = new string[] { "msc", "spb" };
                var cultures = new string[] { "ru-ru", "en-us" };

                var result = matcher.Match("http://bee.ru/ru-ru/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://bee.ru/qwerty", result.SanitizedUrl.ToString());

                result = matcher.Match("/ru-ru/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);

                result = matcher.Match("http://bee.ru/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://bee.ru/qwerty", result.SanitizedUrl.ToString());

                result = matcher.Match("/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);

                result = matcher.Match("http://bee.ru/en-us/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("en-us", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("http://bee.ru/qwerty", result.SanitizedUrl.ToString());
                result = matcher.Match("/en-us/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("en-us", result.Culture);
                Assert.AreEqual("msc", result.Region);


                result = matcher.Match("msc/en-us/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("msc", result.Region);
                Assert.AreEqual("/en-us/msc/qwerty", result.SanitizedUrl.ToString());

                result = matcher.Match("en-us/123/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.IsNull(result.Region);
                Assert.AreEqual("en-us", result.Culture);

                result = matcher.Match("/erf/en-us/123/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.IsNull(result.Region);
                Assert.AreEqual("ru-ru", result.Culture);

                result = matcher.Match("/", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.IsNull(result.Region);
                Assert.AreEqual("ru-ru", result.Culture);


                result = matcher.Match("1231/msc/msc/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.IsNull(result.Region);
            }
        }


        [TestMethod]
        public void Test_RegionMatcher_replace_dns()
        {
            lock (Locker.ForType<Url>())
            {
                var config = new CultureUrlParserConfig();

                config.MatchingPatterns.Add(
                    new UrlMatchingPattern
                    {
                        Value = "{region}.bee.ru/{culture}",
                        DefaultCultureToken = "ru-ru"
                    });

                UrlTokenMatcher matcher = new UrlTokenMatcher(config);


                var regions = new string[] { "msc", "spb" };
                var cultures = new string[] { "ru-ru", "en-us" };

                AssertEquals("http://msc.bee.ru/ru-ru/asdr/qwe", "http://spb.bee.ru/en-us/asdr/qwe", "spb", "en-us",
                    regions, cultures, matcher);

                AssertEquals("http://msc.bee.ru/en-us/asdr/qwe", "http://spb.bee.ru/asdr/qwe", "spb", "ru-ru",
                    regions, cultures, matcher);

                AssertEquals("http://bee.ru/asdr/qwe", "http://spb.bee.ru/asdr/qwe", "spb", "ru-ru",
                    regions, cultures, matcher);

                AssertEquals("/asdr/qwe", "/asdr/qwe", "spb", "ru-ru",
                  regions, cultures, matcher);

                AssertEquals("/asdr/qwe", "/en-us/asdr/qwe", "spb", "en-us",
                 regions, cultures, matcher);

                AssertEquals("http://stage.bee.ru/en-us/asdr/qwe", "http://stage.bee.ru/en-us/asdr/qwe", "spb", "ru-ru",
                    regions, cultures, matcher);

            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_RegionMatcher_replace_segments()
        {
            lock (Locker.ForType<Url>())
            {
                var config = new CultureUrlParserConfig();

                config.MatchingPatterns.Add(
                    new UrlMatchingPattern
                    {
                        Value = "bee.ru/{culture}/{region}",
                        DefaultCultureToken = "ru-ru"
                    });

                config.MatchingPatterns.Add(
                    new UrlMatchingPattern
                    {
                        Value = "bee.ru/{region}",
                        DefaultCultureToken = "ru-ru"
                    });

                UrlTokenMatcher matcher = new UrlTokenMatcher(config);


                var regions = new string[] { "msc", "spb" };
                var cultures = new string[] { "ru-ru", "en-us" };

                AssertEquals("http://bee.ru/ru-ru/msc/asdr/qwe", "http://bee.ru/en-us/spb/asdr/qwe", "spb", "en-us",
                    regions, cultures, matcher);

                AssertEquals("http://bee.ru/en-us/asdr/qwe", "http://bee.ru/spb/asdr/qwe", "spb", "ru-ru",
                    regions, cultures, matcher);

                AssertEquals("http://bee.ru/en-us/msc/qwe", "http://bee.ru/qwe", "", "ru-ru",
                    regions, cultures, matcher);


                AssertEquals("http://bee.ru/asdr/qwe", "http://bee.ru/spb/asdr/qwe", "spb", "ru-ru",
                    regions, cultures, matcher);

                AssertEquals("/asdr/qwe", "/spb/asdr/qwe", "spb", "ru-ru",
                  regions, cultures, matcher);

                AssertEquals("/asdr/qwe", "/en-us/spb/asdr/qwe", "spb", "en-us",
                 regions, cultures, matcher);

            }
        }

        [TestMethod]
        public void Test_RegionMatcher_dns_positive_issue()
        {
            lock (Locker.ForType<Url>())
            {
                var config = new CultureUrlParserConfig();

                config.MatchingPatterns.Add(
                    new UrlMatchingPattern
                    {
                        Value = "{region}.bee.ru/{culture}",
                        DefaultCultureToken = "ru-ru"
                    });

                UrlTokenMatcher matcher = new UrlTokenMatcher(config);

                var regions = new string[] { "moskovskaya-obl", "spb" };
                var cultures = new string[] { "ru-ru", "en-us" };

                var result = matcher.Match("http://moskovskaya-obl.bee.ru/ru-ru/qwerty", regions, cultures);
                Assert.IsTrue(result.IsMatch);
                Assert.AreEqual("ru-ru", result.Culture);
                Assert.AreEqual("moskovskaya-obl", result.Region);
                Assert.AreEqual("http://moskovskaya-obl.bee.ru/qwerty", result.SanitizedUrl.ToString());

                var url = matcher.ReplaceTokens("http://moskovskaya-obl.bee.ru/ru-ru/qwerty", regions, cultures, "ru-ru", "spb", true);
            }
        }

        private static void AssertEquals(Url origin, Url expect, string r, string c, string[] regions, string[] cultures, UrlTokenMatcher matcher)
        {
            Url actual = matcher.ReplaceTokens(origin, regions, cultures, c, r, true);

            Assert.AreEqual<string>(expect, actual);
        }
    }
}


