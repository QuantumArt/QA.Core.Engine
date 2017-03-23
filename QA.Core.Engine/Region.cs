using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Engine
{
    public class Region
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Region))
            {
                return false;
            }

            return ((Region)obj).Id == Id;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
