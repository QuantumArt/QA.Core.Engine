using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Engine.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AllowedZonesAttribute : Attribute, IInheritDefinitionModifier
    {
        public string ZoneNames { get; set; }

        public AllowedZonesAttribute(string zoneNames)
        {
            ZoneNames = zoneNames;
        }

        #region IDefinitionModifier
        void IInheritDefinitionModifier.Modify(ItemDefinition definition, IEnumerable<ItemDefinition> all)
        {
            var zoneNames =  SplitString(ZoneNames, ',');

            if (zoneNames.Length > 0)
            {
                foreach (var zoneName in zoneNames)
                {
                    if (!definition.AllowedZoneNames.Contains(zoneName))
                    {
                        definition.AllowedZoneNames.Add(zoneName); 
                    }
                }
            }
        }
        #endregion

        public string[] SplitString(string str, params char[] separator)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str.Split(separator).Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            return new string[] { };
        }
    }
}
