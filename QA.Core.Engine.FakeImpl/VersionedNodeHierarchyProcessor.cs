using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QA.Core.Engine.Web;
#pragma warning disable 1591

namespace QA.Core.Engine.Data
{
    /// <summary>
    /// Постобработка стурктуры сайта.
    /// </summary>
    public class VersionedNodeHierarchyProcessor : INodeHierarchyProcessor
    {
        private ICultureUrlResolver _resolver;
        public VersionedNodeHierarchyProcessor(ICultureUrlResolver resolver)
        {
            _resolver = resolver;
        }

        public void ProcessNodesChildren(AbstractItem node, IEnumerable<AbstractItem> candidates)
        {
            var groups = candidates.GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (var group in groups)
            {
                var items = group.ToList();
                if (items.Count == 1)
                {
                    node.Children.Add(items[0]);
                }
                else
                {
                    // get culture,
                    // get region

                    var culture = _resolver.GetCurrentCulture();
                    var region = _resolver.GetCurrentRegion();

                    // filter by culture and region

                    var filtred = items.Where(x => string.Equals(x.CultureToken, culture, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    if (string.IsNullOrEmpty(region))
                        filtred = filtred.Where(x => !x.Regions.Any()).ToArray();
                    else
                        filtred = filtred.Where(x => x.Regions.Select(n => n.Alias).Contains(region, StringComparer.InvariantCultureIgnoreCase)).ToArray();

                    if (filtred.Any())
                    {
                        // find structural nodes
                        var structural = filtred.FirstOrDefault();

                        if (structural == null)
                        {
                            Debug.WriteLine("There is no structural node for " + group.Key);
                            continue;
                        }

                        if (structural.VersionOf != null)
                        {
                            structural.VersionOfId = structural.VersionOf.Id;
                            structural.VersionOf = null;
                        }

                        structural.ParentId = node.Id;
                        structural.Parent = node;
                        node.Children.Add(structural);

                        // find content-replacemnts

                        // get actual structural node

                        // set content, if needed

                        // remove "VersionOf"

                        // add one such child or none of them
                    }
                }
            }
        }

        public bool CheckNode(AbstractItem parentNode, AbstractItem nodeToCheck)
        {
            return true;
        }
    }
}
