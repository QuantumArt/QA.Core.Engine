using System;

namespace QA.Core.Engine.Web
{
    public class ItemEventArgs : EventArgs
    {
        private AbstractItem affectedItem;

        public ItemEventArgs(AbstractItem item)
        {
            this.affectedItem = item;
        }
        public AbstractItem AffectedItem
        {
            get { return affectedItem; }
            set { affectedItem = value; }
        }
    }
}
