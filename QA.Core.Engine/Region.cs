using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    public class Region : IEquatable<Region>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public int? Parent { get; set; }

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

            if (object.ReferenceEquals(obj, this)) return true;

            return ((Region)obj).Id == Id;
        }

        public override string ToString()
        {
            return Title;
        }

        public bool Equals(Region other)
        {
            if (other != null) return false;
            if (object.ReferenceEquals(other, this)) return true;
            return Id == other.Id;
        }
    }

    public class HierarchyRegion : Region
    {
        public HierarchyRegion() : base()
        {

        }
        public HierarchyRegion(Region region, int level)
        {
            Id = region.Id;
            Alias = region.Alias;
            Parent = region.Parent;
            Title = region.Title;
            Level = level;
        }
        public int Level { get; set; }
    }
}
