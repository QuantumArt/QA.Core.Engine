using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Engine.Collections;

namespace QA.Core.Engine
{
    /// <summary>
    /// Фильтр для структурного версионирования
    /// </summary>
    public class VersioningFilter : ItemFilter
    {
        private string _currentRegion;
        private string _cultureKey;
        private bool _onlyPages;

        public VersioningFilter(string currentRegion, string cultureKey, bool onlyPages = true)
        {
            _currentRegion = currentRegion;
            _cultureKey = cultureKey;
            _onlyPages = onlyPages;
        }

        public override bool Match(Core.Engine.AbstractItem item)
        {
            bool result = FilterItem(item);

            return result;
        }

        private bool FilterItem(Core.Engine.AbstractItem item)
        {
            bool result = true;
            if (_currentRegion != null && item.Regions.Count > 0)
            {
                result &= item.Regions.Any(x => _currentRegion
                    .Equals(x.Alias, StringComparison.InvariantCultureIgnoreCase));
            }

            if (_cultureKey != null && item.Culture != null)
            {
                result &= _cultureKey
                    .Equals(item.Culture.Key, StringComparison.OrdinalIgnoreCase);
            }

            if (_onlyPages)
            {
                result &= item.IsPage;
            }
            return result;
        }

        public override IEnumerable<T> Pipe<T>(IEnumerable<T> items)
        {
            var set = new Dictionary<string, T>();
            var filtered = base.Pipe<T>(items).ToList();

            var gr = filtered
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new { x.Key, Items = x.ToList() })
                .Where(x => x.Items.Count > 1);

            foreach (var g in gr)
            {
                if (string.IsNullOrEmpty(_currentRegion))
                {
                    // если не определен регион пользователя, то подходят страницы, у которых:
                    // 1) не указаны регионы и указана нужная культура
                    // 2) не указаны регионы и не указана культура
                    // именно  в таком порядке

                    // проверяем 1й пункт
                    var chosen = g
                        .Items
                        .Where(x => x.CultureToken == _cultureKey)
                        .Where(x => x.Regions.Count == 0)
                        .FirstOrDefault();

                    if (chosen == null)
                    {
                        // проверяем 2й пункт
                        chosen = g
                            .Items
                            .Where(x => x.Regions.Count == 0)
                            .FirstOrDefault();
                    }

                    filtered.RemoveAll(x => g.Items.Contains(x) && x != chosen);
                    continue;
                }
                else
                {
                    // если определен регион пользователя, то показываем странцы с таким же регионом, страницы без указанных регионов
                    // с указанной или не указанной культурой

                    // прямое соответствие культуры и региона
                    var chosen = g
                        .Items
                        .Where(x => x.CultureToken == _cultureKey)
                        .Where(x => x.Regions.Count > 0)
                        .FirstOrDefault();

                    // прямое соответствие культуры (регион может быть не указан)
                    if (chosen == null)
                    {
                        chosen = g
                        .Items
                        .Where(x => x.CultureToken == _cultureKey)
                        .FirstOrDefault();
                    }

                    // прямое соответствие региона, культура не указана
                    if (chosen == null)
                    {
                        chosen = g
                        .Items
                        .Where(x => x.CultureToken == null)
                        .Where(x => x.Regions.Count > 0)
                        .FirstOrDefault();
                    }

                    // если не указан регион и не указана культура
                    if (chosen == null)
                    {
                        chosen = g.Items
                            .Where(x => x.CultureToken == null)
                            .Where(x => x.Regions.Count == 0)
                            .FirstOrDefault();
                    }

                    // берем первый попавшийся из подошедших
                    if (chosen == null)
                    {
                        chosen = g.Items.FirstOrDefault();
                        Debug.WriteLine("Две одинаковые страницы." + chosen.Id);
                    }

                    filtered.RemoveAll(x => g.Items.Contains(x) && x != chosen);
                    continue;
                }
            }

            return filtered;
        }
    }
}
