using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.Engine.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Data.Structure
{
    public static class SiteStructure
    {
        internal static bool IsInitialized { get; private set; }
        internal static List<AbstractItem> Items = new List<AbstractItem>();
        static Random _rand = new Random(Guid.NewGuid().GetHashCode());
        private static FakeStorage _storage;

        #region Zones
        public const string LeftZone = "Left";
        public const string RightZone = "Right";
        public const string CenterZone = "Center";
        public const string TopZone = "Top";
        public const string BottomZone = "Bottom";
        public const string FooterZone = "Footer";
        public const string SiteLeftZone = "SiteLeft";
        public const string SiteRightZone = "SiteRight";
        public const string SiteBottomZone = "SiteBottom";
        public const string SiteFooterZone = "SiteFooter";
        #endregion

        internal static void AttachTo(FakeStorage storage)
        {
            _storage = storage;
        }

        public static void CompleteInitializing()
        {
            foreach (var item in Items)
            {
                if (item is IInjectable<IUrlParser>)
                {
                    ((IInjectable<IUrlParser>)item).Set(ObjectFactoryBase.Resolve<IUrlParser>());
                }
            }

            _storage.InitializeStorage();
        }

        /// <summary>
        /// Генерация произвольного html-текста с картинками
        /// </summary>
        /// <param name="maxLevel"></param>
        /// <returns></returns>
        public static string GenerateRandomContent(int maxLevel)
        {
            var tags = new string[] { "p", "strong", "span", "font", "i", "div", "h4", "p" };


            #region Lorem Ipsum
            var texts = new string[] { "Ut wisi enim ad minim veniam, Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat ",
                "Lorem luptatum zzril delenit augue duis dolore te feugait nulla facilisi. ",
                "Sed diam nonummy nibh euismod tincidunt ut laoreet. ",
                "Сегодня этот текст используют практически все дизайнеры, набирающие рыбу латиницей. ",
                "Абзац считается каноническим во всех справочниках по типографике и предлагается к использованию в статьях, посвященных изготовлению макета верстки при отсутствии финальных текстов. ",
                "Желающим использовать в своей верстке именно неправильный вариант следует убедиться, что в нем нет ничего непристойного — за годы модификации этого рыбного текста многие шутники чего только не повписывали туда. ",
                "Так как никто не может знать, когда и где предложение будет разбито на строчки, нужно обрабатывать все случаи. На строчке не могут остаться: инициалы, одно-, двух- и некоторая часть трехбуквенных слов, цифры года. На строчке должно остаться длинное тире. ",
                "Строгих правил по поводу переноса слов нет, все они носят рекомендательный характер. Каждый раз нужно вникать в смысл текста и привязывать предлоги и союзы к следующему за ними слову, а частицы — к предыдущему. ",
                @"<img src=""http://quantumart.ru/upload/contents/229/design.gif""  alt=""ico"" style=""border:none;""> ",
                @"<img src=""http://quantumart.ru/upload/contents/229/promosite.gif"" alt=""ico"" style=""border:none;""> ",
                @"<img src=""http://quantumart.ru/upload/images/banner_dryer_003_rus.jpg"" width=""150"" alt=""ico"" style=""border:none;""> ",
                "<ul><li>Пункт1 Абзац считается каноническим</li><li>пункт2</li><li>пункт3 Lorem <strong>luptatum zzril</strong></li></ul> ",
                "<ul><li>Пункт1</li><li>пункт123</li><li>пункт2333322222 Lorem <strong>luptatum zzril</strong></li></ul> ",
                "<br />"
            };
            #endregion

            var sb = new StringBuilder();

            GenerateRecursive(tags, texts, sb, -maxLevel);

            return sb.ToString();
        }

        private static void GenerateRecursive(string[] tags, string[] texts, StringBuilder sb, int level)
        {
            if (level >= 0)
            {
                return;
            }

            level += 1;

            var tag = tags[_rand.Next(0, tags.Length - 1)];
            var text = texts[_rand.Next(0, texts.Length - 1)];

            sb.AppendFormat("<{0}>", tag);
            sb.AppendFormat("{0}", text);

            if (_rand.Next() % 2 == 0)
            {
                GenerateRecursive(tags, texts, sb, level);
            }
            sb.AppendFormat("</{0}>", tag);

            if (_rand.Next() % 2 == 0)
            {
                GenerateRecursive(tags, texts, sb, level);
            }
        }

        public static void Initialize(IRootPage rootPage, IStartPage startPage, Action<AbstractItem> init, bool skipIds = false)
        {
            if (!skipIds)
            {
                ((AbstractItem)rootPage).Id = 1796;
                ((AbstractItem)startPage).Id = 1703;
                ((AbstractItem)rootPage).IsPage = true;
                ((AbstractItem)startPage).IsPage = true;

                ((AbstractItem)rootPage).Name = "root";
                ((AbstractItem)startPage).Name = "home";
            }

            Items.Add((AbstractItem)rootPage);
            Items.Add((AbstractItem)startPage);

            ((AbstractItem)rootPage).AddChild("home", (AbstractItem)startPage);

            init((AbstractItem)startPage);

            IsInitialized = true;
        }

        public static AbstractItem AddChild(this AbstractItem parent, string name, AbstractItem child, Action<AbstractItem> withChild = null)
        {
            if (!parent.Children.Contains(child))
            {
                parent.Children.Add(child);
            }

            child.Parent = parent;

            if (child != null)
            {
                child.Name = name;
            }

            if (child.Id == 0)
            {
                child.Id = Items.Count + 1;
            }

            if (child.Name == null)
            {
                child.Name = child.Id.ToString();
            }

            if (string.IsNullOrEmpty(child.Title))
            {
                child.Title = child.Name;
            }

            child.IsPage = true;
            child.IsVisible = true;

            if (!Items.Contains(child))
            {
                Items.Add(child);
            }

            if (withChild != null)
            {
                withChild(child);
            }


            return parent;
        }

        public static AbstractItem AddChild(this AbstractItem parent, AbstractItem child, Action<AbstractItem> withChild = null)
        {
            return parent.AddChild(null, child, withChild);
        }

        public static AbstractItem AddWidget(this AbstractItem parent, string zoneName, AbstractItem child, Action<AbstractItem> withChild = null)
        {
            if (!parent.Children.Contains(child))
            {
                parent.Children.Add(child);
            }

            child.Parent = parent;

            if (child != null)
            {
                child.ZoneName = zoneName;
            }

            if (string.IsNullOrEmpty(child.Title))
            {
                child.Title = child.Name;
            }

            if (child.Id == 0)
            {
                child.Id = Items.Count + 1;
            }

            if (child.Name == null)
            {
                child.Name = child.Id.ToString();
            }

            if (!Items.Contains(child))
            {
                Items.Add(child);
            }

            if (withChild != null)
            {
                withChild(child);
            }
            return parent;
        }
    }
}
