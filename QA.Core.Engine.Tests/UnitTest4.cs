using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Filters;
using QA.Core.Engine.Interface;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class UnitTest4
    {
        [TestMethod]
        public void Test_UrlPartFilter_allowed_simple()
        {
            var filter = new UrlPartFilter("c/a");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "c/a" }
            };

            Assert.IsTrue(filter.MatchUrl("c/a", item));
            Assert.IsTrue(filter.MatchUrl("c/a/", item));
            Assert.IsTrue(filter.MatchUrl("/c/a/", item));
            Assert.IsTrue(filter.MatchUrl("/c/a", item));

            Assert.IsTrue(filter.MatchUrl("c/a?123", item));
            Assert.IsTrue(filter.MatchUrl("c/a/?123", item));

            Assert.IsFalse(filter.MatchUrl("/c/a/d", item));
            Assert.IsFalse(filter.MatchUrl("a/c/a", item));
            Assert.IsFalse(filter.MatchUrl("/a/c/a", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_denied_simple()
        {
            var filter = new UrlPartFilter("c/a");

            var item = new StubItem()
            {
                DeniedUrls = new string[] { "c/a" }
            };

            Assert.IsFalse(filter.MatchUrl("c/a", item));
            Assert.IsFalse(filter.MatchUrl("c/a/", item));
            Assert.IsFalse(filter.MatchUrl("/c/a/", item));
            Assert.IsFalse(filter.MatchUrl("/c/a", item));

            Assert.IsFalse(filter.MatchUrl("c/a?123", item));
            Assert.IsFalse(filter.MatchUrl("c/a/?123", item));

            Assert.IsTrue(filter.MatchUrl("/c/a/d", item));
            Assert.IsTrue(filter.MatchUrl("a/c/a", item));
            Assert.IsTrue(filter.MatchUrl("/a/c/a", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_allowed_simple_with_query()
        {
            var filter = new UrlPartFilter("c/a");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "c/a" }
            };

            Assert.IsTrue(filter.MatchUrl("c/a?123", item));
            Assert.IsTrue(filter.MatchUrl("c/a/?123", item));
            Assert.IsTrue(filter.MatchUrl("/c/a/?123", item));
            Assert.IsTrue(filter.MatchUrl("/c/a?123", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_allowed_wildcard()
        {
            var filter = new UrlPartFilter("c/a/*");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "c/a/*" }
            };

            Assert.IsTrue(filter.MatchUrl("c/a", item));
            Assert.IsTrue(filter.MatchUrl("c/a/", item));
            Assert.IsTrue(filter.MatchUrl("/c/a/", item));
            Assert.IsTrue(filter.MatchUrl("/c/a", item));

            Assert.IsTrue(filter.MatchUrl("/c/a/d", item));
            Assert.IsTrue(filter.MatchUrl("c/a/d", item));

            Assert.IsFalse(filter.MatchUrl("a/c/a", item));
            Assert.IsFalse(filter.MatchUrl("/a/c/a", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_denied_wildcard()
        {
            var filter = new UrlPartFilter("c/a/*");

            var item = new StubItem()
            {
                DeniedUrls = new string[] { "c/a/*" }
            };

            Assert.IsFalse(filter.MatchUrl("c/a", item));
            Assert.IsFalse(filter.MatchUrl("c/a/", item));
            Assert.IsFalse(filter.MatchUrl("/c/a/", item));
            Assert.IsFalse(filter.MatchUrl("/c/a", item));

            Assert.IsFalse(filter.MatchUrl("/c/a/d", item));
            Assert.IsFalse(filter.MatchUrl("c/a/d", item));

            Assert.IsTrue(filter.MatchUrl("a/c/a", item));
            Assert.IsTrue(filter.MatchUrl("/a/c/a", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_allowed_wildcard_with_query()
        {
            var filter = new UrlPartFilter("c/a/*");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "c/a/*" }
            };

            Assert.IsTrue(filter.MatchUrl("c/a?123", item));
            Assert.IsTrue(filter.MatchUrl("c/a/", item));
            Assert.IsTrue(filter.MatchUrl("/c/a/?123", item));
            Assert.IsTrue(filter.MatchUrl("/c/a", item));

            Assert.IsTrue(filter.MatchUrl("/c/a/d?123", item));
            Assert.IsTrue(filter.MatchUrl("c/a/d?123", item));

            Assert.IsFalse(filter.MatchUrl("a/c/a", item));
            Assert.IsFalse(filter.MatchUrl("/a/c/a", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_allowed_matches_all()
        {
            var filter = new UrlPartFilter("customers/products/");

            var item = new StubItem()
            {
            };

            Assert.IsTrue(filter.MatchUrl("customers/products/", item));
            Assert.IsTrue(filter.MatchUrl("customers/12312/sad", item));
        }

        [TestMethod]
        public void Test_UrlPartFilter_allowed_denied_multiple()
        {
            var filter = new UrlPartFilter("customers/products/");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "a/b", "b/c*", "c/d/e/f" },
                DeniedUrls = new string[] {"c/d", "b/c/d", "c/d/*" },
            };

            Assert.IsTrue(filter.MatchUrl("a/b", item));
            Assert.IsTrue(filter.MatchUrl("b/c/we/e", item));

            Assert.IsFalse(filter.MatchUrl("c/d", item));
            Assert.IsFalse(filter.MatchUrl("c/d/", item));
            Assert.IsFalse(filter.MatchUrl("c/d/2", item));
        }

        //[TestMethod]
        // пока убрали
        public void Test_UrlPartFilter_allowed_denied_multiple2()
        {
            var filter = new UrlPartFilter("customers/products/");

            var item = new StubItem()
            {
                AllowedUrls = new string[] { "a/b", "b/c*", "c/d/e/f" },
                DeniedUrls = new string[] { "c/d", "b/c/d", "c/d/*" },
            };

            Assert.IsTrue(filter.MatchUrl("c/d/e/f", item));

        }

        class StubItem : IUrlFilterable
        {
            public System.Collections.Generic.IEnumerable<string> AllowedUrls
            {
                get;
                set;
            }

            public System.Collections.Generic.IEnumerable<string> DeniedUrls
            {
                get;
                set;
            }

        }
    }
}
