using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class UrlTrailiing_Tests
    {
        [TestMethod]
        public void Url_Tests()
        {
            lock (Locker.ForType<Url>())
            {
                var url = new Url("/home?param=1")
                    .AppendSegment("details")
                    .AppendQuery("param2", 2)
                    .AppendQuery("param3", "abc")
                    .AppendQuery("param4", "dce");

                Assert.AreEqual("/home/details?param=1&param2=2&param3=abc&param4=dce", url.ToString());
            }
        }

        [TestMethod]
        public void Url_Issue_Tests11()
        {
            lock (Locker.ForType<Url>())
            {
                var url = new Url("http://asd.ru/home/asd.doc", true)
                    .AppendQuery("param2", 2);

                Assert.AreEqual("http://asd.ru/home/asd.doc?param2=2", url.ToString());
            }
        }

        [TestMethod]
        public void Url_Tests1()
        {
            lock (Locker.ForType<Url>())
            {
                var url = new Url("/home/internet/help?param=1&param1")
                    .AppendSegment("details")
                    .AppendQuery("param2", 2)
                    .AppendQuery("param3", "abc")
                    .AppendQuery("param4", "dce");

                Assert.AreEqual("/home/internet/help/details?param=1&param1&param2=2&param3=abc&param4=dce", url.ToString());
            }
        }


        [TestMethod]
        public void Url_Tests2()
        {

            lock (Locker.ForType<Url>())
            {

                var url = new Url("/home/internet/?param=1&param1")
                    .AppendSegment("help/details")
                    .AppendQuery("param2", 2)
                    .AppendQuery("param3", "abc")
                    .AppendQuery("param4", "dce");

                Assert.AreEqual("/home/internet/help/details?param=1&param1&param2=2&param3=abc&param4=dce", url.ToString());
            }
        }

        [TestMethod]
        public void Url_TrailingTest()
        {
            lock (Locker.ForType<Url>())
            {
                var old = Url.AddTrailingSlashes;
                try
                {
                    Url.AddTrailingSlashes = true;

                    CheckEndsWithSlash(Url.Parse("~/test"));
                    CheckEndsWithSlash(Url.Parse("~/test").Path);
                    CheckEndsWithSlash(Url.Parse("~/test"));
                    CheckEndsWithSlash(Url.Parse("~/test/"));
                    CheckEndsWithSlash(Url.Parse("~/test/123123"));
                    CheckEndsWithSlash(Url.Parse("~/test/123123").Path);
                    CheckEndsWithSlash(Url.Parse("http://test/123123"));
                    CheckEndsWithSlash(Url.Parse("http://domain.ru/test/123123"));
                    CheckEndsWithSlash(Url.Parse("http://domain.ru/test/123123").Path);
                    CheckEndsWithSlash(Url.Parse("http://domain.ru"));
                    CheckEndsWithSlash(Url.Parse("http://domain.ru").Path);
                }
                finally
                {
                    Url.AddTrailingSlashes = old;
                }
            }
        }

        [TestMethod]
        public void Url_TrailingTest_this_override()
        {
            lock (Locker.ForType<Url>())
            {
                var old = Url.AddTrailingSlashes;
                try
                {
                    Url.AddTrailingSlashes = true;

                    CheckDoesnotEndWithSlash(Url.Parse("~/test/", false));
                    CheckDoesnotEndWithSlash(Url.Parse("~/test", false).Path);
                    CheckDoesnotEndWithSlash(Url.Parse("~/test", false));
                    CheckDoesnotEndWithSlash(Url.Parse("~/test/", false));
                    CheckDoesnotEndWithSlash(Url.Parse("~/test/123123", false));
                    CheckDoesnotEndWithSlash(Url.Parse("~/test/123123", false).Path);
                    CheckDoesnotEndWithSlash(Url.Parse("http://test/123123", false));
                    CheckDoesnotEndWithSlash(Url.Parse("http://domain.ru/test/123123", false));
                    CheckDoesnotEndWithSlash(Url.Parse("http://domain.ru/test/123123", false).Path);
                    CheckDoesnotEndWithSlash(Url.Parse("http://domain.ru", false));
                    Assert.AreEqual("", Url.Parse("http://domain.ru", false).Path);
                }
                finally
                {
                    Url.AddTrailingSlashes = old;
                }
            }
        }

        [TestMethod]
        public void Url_TrailingTest_With_Query()
        {
            lock (Locker.ForType<Url>())
            {
                var old = Url.AddTrailingSlashes;
                try
                {
                    Url.AddTrailingSlashes = true;

                    CheckEndsWithSlash(Url.Parse("~/test?asd4").Path);
                    CheckEndsWithSlash(Url.Parse("~/test/123123").Path);
                    CheckEndsWithSlash(Url.Parse("http://test/123123/?asda=asd").Path);
                    CheckEndsWithSlash(Url.Parse("http://domain.ru/test/123123?asda=123&1231=123").Path);
                    CheckEndsWithSlash(Url.Parse("http://domain.ru?asd=agfs&123").Path);
                }
                finally
                {
                    Url.AddTrailingSlashes = old;
                }
            }
        }


        [TestMethod]
        public void Url_TrailingTest_complex()
        {
            lock (Locker.ForType<Url>())
            {
                var old = Url.AddTrailingSlashes;
                try
                {
                    Url.AddTrailingSlashes = true;

                    var url = Url.Parse("http://domain.ru/", false)
                        .SetAuthority("domain2.com")
                        .SetQueryParameter("1", 2)
                        .SetScheme("https")
                        .SetPath("test/test2")
                        .AppendSegment("test3")
                        .PrependSegment("test0");

                    CheckDoesnotEndWithSlash(url.Path);

                    Assert.AreEqual("https://domain2.com/test0/test/test2/test3?1=2", url.ToString());
                }
                finally
                {
                    Url.AddTrailingSlashes = old;
                }
            }
        }

        [TestMethod]
        public void Url_TrailingTest_Issue01()
        {
            lock (Locker.ForType<Url>())
            {
                var old = Url.AddTrailingSlashes;
                try
                {
                    Url.AddTrailingSlashes = true;
                    Assert.AreEqual<string>("http://asd.ru/123/?asd=123#test", new Url("Http", "asd.ru", "/123", "asd=123", "test").ToString().ToLower());
                }
                finally
                {
                    Url.AddTrailingSlashes = old;
                }
            }
        }

        [TestMethod]
        public void Url_TrailingTest_Issue02()
        {
            Url url = "http://evil.com";

            ConvertUrlToSchemaInvariant(url);

        }

        private static string ConvertUrlToSchemaInvariant(string prefix)
        {
            if (prefix.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                prefix = "//" + prefix.Substring(7);
            }
            return prefix;
        }

        public static string CheckEndsWithSlash(string url)
        {
            Assert.IsTrue(url.EndsWith("/"));
            return url;
        }

        public static string CheckDoesnotEndWithSlash(string url)
        {
            Assert.IsFalse(url.EndsWith("/"));
            return url;
        }
    }
}
