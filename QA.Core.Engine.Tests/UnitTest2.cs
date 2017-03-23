using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Engine.Data;

namespace QA.Core.Engine.Tests
{
    [TestClass]
    public class UnitTest2
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
    }
}
