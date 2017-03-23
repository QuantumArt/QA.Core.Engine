using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.Engine.UI;

namespace QA.Core.Engine.Data
{
    public class FakeEditUrlManager : IEditUrlManager
    {
        public string GetEditExistingItemUrl(AbstractItem item)
        {
            return "/cms/managment/editnode?id=" + item.Id;
        }

        public string GetDeleteUrl(AbstractItem item)
        {
            return "/cms/managment/deletenode?id=" + item.Id;
        }


        public string GetBaseNavigationUrl()
        {
            return "/cms/managment";
        }

        public string GetCreateUrl()
        {
            return "";
        }
    }
}
