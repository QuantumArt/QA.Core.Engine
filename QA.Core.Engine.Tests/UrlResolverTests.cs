using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Data;
using QA.Core.Engine.FakeImpl;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class UrlResolverTests
    {
        [TestMethod]
        public void Test_Urlresolver_resolving()
        {
            lock (Locker.ForType<Url>())
            {
                var urlresolver = new CultureUrlResolver();

                string r, c = null;

                urlresolver.ResolveCulture("http://asd.ru/", out c, out r, false);

                Assert.AreEqual("ru-ru", c);
                Assert.AreEqual(null, r);

                urlresolver.ResolveCulture("http://asd.ru/123/123/123", out c, out r, false);

                Assert.AreEqual("ru-ru", c);
                Assert.AreEqual(null, r);

                urlresolver.ResolveCulture("http://asd.ru/en-us", out c, out r, false);

                Assert.AreEqual("en-us", c);
                Assert.AreEqual(null, r);

                urlresolver.ResolveCulture("http://asd.ru/en-us/ru-ru/123/123/", out c, out r, false);

                Assert.AreEqual("en-us", c);
                Assert.AreEqual(null, r);


                urlresolver.ResolveCulture("http://asd.ru/en-us/spb", out c, out r, false);

                Assert.AreEqual("en-us", c);
                Assert.AreEqual("spb", r);

                urlresolver.ResolveCulture("http://asd.ru/msc/spb", out c, out r, false);

                Assert.AreEqual("ru-ru", c);
                Assert.AreEqual("msc", r);

                urlresolver.ResolveCulture("http://asd.ru/spb", out c, out r, false);

                Assert.AreEqual("ru-ru", c);
                Assert.AreEqual("spb", r);
            }
        }

        [TestMethod]
        public void Test_Urlresolver_adding()
        {
            lock (Locker.ForType<Url>())
            {
                var urlresolver = new CultureUrlResolver();

                Url url = null;

                url = urlresolver.AddTokensToUrl("http://asd.ru/", "en-us", "spb");
                Assert.AreEqual("http://asd.ru/en-us/spb", url.ToString());


                url = urlresolver.AddTokensToUrl("http://asd.ru/", "ru-ru", "spb");
                Assert.AreEqual("http://asd.ru/spb", url.ToString());
            }
        }

        [TestMethod]
        public void Test_Urlresolver_add_or_replace()
        {
            lock (Locker.ForType<Url>())
            {
                var urlresolver = new CultureUrlResolver();

                Url url = null;

                url = urlresolver.AddTokensToUrl("http://asd.ru/msc", "en-us", "spb", true);
                Assert.AreEqual("http://asd.ru/en-us/spb", url.ToString());


                url = urlresolver.AddTokensToUrl("http://asd.ru/msc", "ru-ru", "spb", true);
                Assert.AreEqual("http://asd.ru/spb", url.ToString());

                url = urlresolver.AddTokensToUrl("http://asd.ru/ru-ru/msc", "en-us", "spb", true);
                Assert.AreEqual("http://asd.ru/en-us/spb", url.ToString());
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_Urlresolver_add_or_replace_complex()
        {
            lock (Locker.ForType<Url>())
            {
                var urlresolver = new CultureUrlResolver();

                Url url = null;

                url = urlresolver.AddTokensToUrl("http://asd.ru/msc/123", "en-us", "spb", true);
                Assert.AreEqual("http://asd.ru/en-us/spb/123", url.ToString());


                url = urlresolver.AddTokensToUrl("http://asd.ru/msc/123", "ru-ru", "spb", true);
                Assert.AreEqual("http://asd.ru/spb/123", url.ToString());

                url = urlresolver.AddTokensToUrl("http://asd.ru/ru-ru/msc/123", "en-us", "spb", true);
                Assert.AreEqual("http://asd.ru/en-us/spb/123", url.ToString());
            }
        }

        [TestMethod]
        public void Test_Urlresolver_add_or_replace_complex1()
        {
            //var urlresolver = new CultureUrlResolver();

            //Url url = null;

            //url = urlresolver.AddTokensToUrl("http://moskva.stage.demo.artq.com/", "ru-ru", "moskovskaya-obl", true);

            //Assert.AreEqual("http://moskovskaya-obl.stage.demo.artq.com/", url.ToString());
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.stage.demo.artq.com/{culture}/",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            lock (Locker.ForType<Url>())
            {
                Url url = null;

                url = urlresolver.AddTokensToUrl("http://moskva.stage.demo.artq.com/", "ru-ru", "moskovskaya-obl", true);

                Assert.AreEqual("http://moskovskaya-obl.stage.demo.artq.com/", url.ToString());
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_virtualpath()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.localhost/b2b/{culture}",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            lock (Locker.ForType<Url>())
            {
                bool f = Url.AddTrailingSlashes;
                try
                {

                    Url.AddTrailingSlashes = true;
                    Url url = null;

                    url = urlresolver.AddTokensToUrl("http://moskva.localhost/b2b/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/en-us/test/", url.ToString());
                }
                finally
                {
                    Url.AddTrailingSlashes = f;
                }
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_virtualpath_complex()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.localhost/b2b/{culture}",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            lock (Locker.ForType<Url>())
            {
                bool f = Url.AddTrailingSlashes;
                try
                {

                    Url.AddTrailingSlashes = true;
                    Url url = null;

                    url = urlresolver.AddTokensToUrl("http://moskva.localhost/b2b/ru-ru/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/en-us/test/", url.ToString());

                    url = urlresolver.AddTokensToUrl("http://moskovskaya-obl.localhost/b2b/ru-ru/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/en-us/test/", url.ToString());


                    url = urlresolver.AddTokensToUrl("http://moskovskaya-obl.localhost/b2b/fr-fr/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/en-us/test/", url.ToString());


                    url = urlresolver.AddTokensToUrl("http://moskovskaya-obl.localhost/b2b/fr-fr/test/", "ru-ru", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/test/", url.ToString());
                }
                finally
                {
                    Url.AddTrailingSlashes = f;
                }
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_virtualpath_Tokens_InSegments()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "localhost/b2b/{culture}/{region}",
                         DefaultCultureToken = "ru-ru"
                     },
                     new UrlMatchingPattern
                     {
                         Value = "localhost/b2b/{region}",
                         DefaultCultureToken = "ru-ru"
                     }

                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            lock (Locker.ForType<Url>())
            {
                bool f = Url.AddTrailingSlashes;
                try
                {

                    Url.AddTrailingSlashes = true;
                    Url url = null;

                    url = urlresolver.AddTokensToUrl("http://localhost/b2b/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://localhost/b2b/en-us/moskva/test/", url.ToString());

                    url = urlresolver.AddTokensToUrl("http://localhost/b2b/ru-ru/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://localhost/b2b/en-us/moskva/test/", url.ToString());

                    url = urlresolver.AddTokensToUrl("http://localhost/b2b/fr-fr/test/", "en-us", "moskovskaya-obl", true);
                    Assert.AreEqual("http://localhost/b2b/en-us/moskovskaya-obl/test/", url.ToString());

                    url = urlresolver.AddTokensToUrl("http://localhost/b2b/test/", "ru-ru", "moskva", true);
                    Assert.AreEqual("http://localhost/b2b/moskva/test/", url.ToString());

                    url = urlresolver.AddTokensToUrl("http://localhost/b2b/fr-fr/test/", "en-us", "moskva", true);
                    Assert.AreEqual("http://localhost/b2b/en-us/moskva/test/", url.ToString());
                }
                finally
                {
                    Url.AddTrailingSlashes = f;
                }
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_url_queries()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{culture}/{region}",
                         DefaultCultureToken = "ru-ru"
                     },
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{region}",
                         DefaultCultureToken = "ru-ru"
                     }

                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            Url url = new Url("http://localhost/b2b/test/")
                .SetQueryParameter(PathData.RegionQueryKey, "moskva")
                .SetQueryParameter(PathData.CultureQueryKey, "en-us");

            string c, r;

            var cleaned = urlresolver.ResolveCulture(url, out c, out r, true);
            Assert.AreEqual("moskva", r);
            Assert.AreEqual("en-us", c);

            Assert.AreEqual("moskva", urlresolver.GetCurrentRegion());
            Assert.AreEqual("en-us", urlresolver.GetCurrentCulture());

        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_url_queries_override_culture()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{culture}/{region}",
                         DefaultCultureToken = "ru-ru"
                     },
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{region}",
                         DefaultCultureToken = "ru-ru"
                     }

                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            Url url = new Url("http://localhost/b2b/fr-fr/test/")
                .SetQueryParameter(PathData.RegionQueryKey, "moskva")
                .SetQueryParameter(PathData.CultureQueryKey, "en-us");

            string c, r;

            var cleaned = urlresolver.ResolveCulture(url, out c, out r, true);
            Assert.AreEqual("moskva", r);
            Assert.AreEqual("en-us", c);

            Assert.AreEqual("moskva", urlresolver.GetCurrentRegion());
            Assert.AreEqual("en-us", urlresolver.GetCurrentCulture());

        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_url_queries_invalid_region_returns_null()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{culture}/{region}",
                         DefaultCultureToken = "ru-ru"
                     },
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{region}",
                         DefaultCultureToken = "ru-ru"
                     }

                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            Url url = new Url("http://localhost/b2b/test/")
                .SetQueryParameter(PathData.RegionQueryKey, "moskva1")
                .SetQueryParameter(PathData.CultureQueryKey, "en-us");

            string c, r;

            var cleaned = urlresolver.ResolveCulture(url, out c, out r, true);
            Assert.AreEqual(null, r);
            Assert.AreEqual("en-us", c);

            Assert.AreEqual(null, urlresolver.GetCurrentRegion());
            Assert.AreEqual("en-us", urlresolver.GetCurrentCulture());

        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_url_queries_invalid_culture_returns_default()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{culture}/{region}",
                         DefaultCultureToken = "ru-ru"
                     },
                     new UrlMatchingPattern
                     {
                         Value = "localhost/{region}",
                         DefaultCultureToken = "ru-ru"
                     }

                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            Url url = new Url("http://localhost/b2b/test/")
                .SetQueryParameter(PathData.RegionQueryKey, "mos1kva")
                .SetQueryParameter(PathData.CultureQueryKey, "11en2-us11");

            string c, r;

            var cleaned = urlresolver.ResolveCulture(url, out c, out r, true);
            Assert.AreEqual(null, r);
            Assert.AreEqual("ru-ru", c);

            Assert.AreEqual(null, urlresolver.GetCurrentRegion());
            Assert.AreEqual("ru-ru", urlresolver.GetCurrentCulture());

        }



        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_virtualpath_replace_root()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.localhost/b2b/{culture}",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskovskaya-obl" },
                    new Region{ Alias = "moskva" }
                });

            lock (Locker.ForType<Url>())
            {
                bool f = Url.AddTrailingSlashes;
                try
                {

                    Url.AddTrailingSlashes = true;
                    Url url = null;

                    url = urlresolver.AddTokensToUrl("http://moskva.localhost/b2b/en-us/", "ru-ru", "moskva", true);
                    Assert.AreEqual("http://moskva.localhost/b2b/", url.ToString());
                }
                finally
                {
                    Url.AddTrailingSlashes = f;
                }
            }
        }

        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_fails_on_stage()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.stage.dr-bee.ru/{culture}/",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskva" },
                    new Region{ Alias = "spb" },
                    new Region{ Alias = "omsk" }
                });

            lock (Locker.ForType<Url>())
            {
                Url url = "http://stage.dr-bee.ru/";
                string r,c;

                var url1 = urlresolver.ResolveCulture(url, out c, out r, true);

                Assert.AreEqual("ru-ru", c);
                Assert.IsNull(r);

               var newUrl= urlresolver.AddTokensToUrl(url, "ru-ru", "spb", true);

               Assert.AreEqual("http://spb.stage.dr-bee.ru/", newUrl.ToString());
            }
        }
        [TestMethod]
        [TestCategory("Issues")]
        public void Test_CultureUrlResolverMatcher_fails_on_stage1()
        {

            var urlresolver = new CultureUrlResolverMatcher(
                new List<UrlMatchingPattern>
                {
                     new UrlMatchingPattern
                     {
                         Value = "{region}.stage.dr-bee.ru/{culture}/",
                         DefaultCultureToken = "ru-ru"
                     }
                },
                new List<Region>
                {
                    new Region{ Alias = "moskva" },
                    new Region{ Alias = "spb" },
                    new Region{ Alias = "omsk" }
                });

            lock (Locker.ForType<Url>())
            {
                Url url = "http://spb.stage.dr-bee.ru/";
                //string r, c;

                //var url1 = urlresolver.ResolveCulture(url, out c, out r, true);

                //Assert.AreEqual("spb", r);

                var newUrl = urlresolver.AddTokensToUrl(url, "ru-ru", "omsk", true);

                Assert.AreEqual("http://omsk.stage.dr-bee.ru/", newUrl.ToString());
            }
        }
    }
}
