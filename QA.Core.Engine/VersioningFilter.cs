using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QA.Core.Engine.Collections;
using QA.Core.Engine.Interface;
#pragma warning disable 1591

namespace QA.Core.Engine
{
    /// <summary>
    /// Фильтр для структурного версионирования
    /// </summary>
    public class VersioningFilter : ItemFilter
    {
        public static bool? UseRegionsHierarchy = false;
        private readonly string _currentRegion;
        private readonly string _cultureKey;
        private readonly bool _onlyPages;
        private readonly bool _useRegionsHierarchy;
        private static HierarchyRegion[] _empty = new HierarchyRegion[0];
        private readonly HierarchyRegion[] _regions;

        public VersioningFilter(string currentRegion, string cultureKey, bool onlyPages = true, bool? useRegionsHierarchy = null, IRegionHierarchyProvider hierarchyProvider = null)
        {
            _currentRegion = currentRegion;
            _cultureKey = cultureKey;
            _onlyPages = onlyPages;
            _useRegionsHierarchy = (useRegionsHierarchy ?? UseRegionsHierarchy) ?? false;

            if (_useRegionsHierarchy == true)
            {
                if (currentRegion == null)
                {
                    _regions = _empty;
                }
                else
                {
                    hierarchyProvider = hierarchyProvider ?? ObjectFactoryBase.Resolve<IRegionHierarchyProvider>();
                    _regions = hierarchyProvider.GetParentRegionsAndSelf(currentRegion) ?? _empty;
                }
            }
        }

        public override bool Match(Core.Engine.AbstractItem item)
        {
            bool result = FilterItem(item);

            return result;
        }

        private bool FilterItem(Core.Engine.AbstractItem item)
        {
            bool result = true;

            if (_useRegionsHierarchy == true)
            {
                // учитывается вложенность регионов
                if (item.Regions.Count == 0)
                {
                    // елементы без регионов не проходят фильтр, при условии, что текущий регион определен
                    // если регион не определен, то элементы проходят
                    result = (_currentRegion == null);
                }
                else
                {
                    if (_currentRegion == null)
                    {
                        // елементы с регионами, но если текущий регион не определен, проходят
                        result = true;
                    }
                    else
                    {
                        // фильтруем с учетом родительских регионов
                        result &= item.Regions.Any(x => _regions.Contains(x));
                    }
                }
            }
            else
            {
                if (result && _currentRegion != null && item.Regions.Count > 0)
                {
                    result &= item.Regions.Any(x => _currentRegion
                        .Equals(x.Alias, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            if (result && _cultureKey != null && item.Culture != null)
            {
                // если указана культура, то не пропускаем
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
            // отфильтровываем заведомо не подходящие элементы
            var filtered = base.Pipe<T>(items).ToList();

            // группируем по алиасу
            var gr = filtered
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new { x.Key, Items = x.ToList() })
                .Where(x => x.Items.Count > 1);

            foreach (var g in gr)
            {
                // для каждого алиаса
                if (string.IsNullOrEmpty(_currentRegion))
                {
                    // если не определен регион пользователя, то подходят страницы, у которых:
                    // 1) не указаны регионы и указана нужная культура
                    // 2) не указаны регионы и не указана культура
                    // именно  в таком порядке

                    // эта логика сохранена и для иерархий

                    // проверяем 1й пункт
                    AbstractItem chosen = g
                        .Items
                        .Where(x => x.CultureToken == _cultureKey && x.Regions.Count == 0)
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
                    if (_useRegionsHierarchy == true)
                    {
                        // если определен регион, то выбираются те страницы, у которых наиболее специфичный регион с приоритетом явно подходящей культуры.
                        // элементы с не указанным регионом не подходят

                        // сначала отфильтруем по соответствию регионов
                        var rangedItems = g.Items.Select(x => new
                        {
                            Item = x,
                            CultureSpecified = x.Culture != null,
                            Score = _regions
                                .Where(y => _regions.Contains(y))
                                .Max(y => y.Level)
                        })
                            .OrderBy(x => x.Score)
                            .ToArray();

                        // наибольший приоритет по весу регионов c указанной подходящей культурой
                        var candidate = rangedItems.Where(x => x.CultureSpecified).FirstOrDefault();

                        if (candidate == null)
                        {
                            candidate = rangedItems.FirstOrDefault();
                        }

                        filtered.RemoveAll(x => g.Items.Contains(x) && x != candidate?.Item);
                        continue;
                    }
                    else
                    {
                        // если определен регион пользователя, то показываем странцы с таким же регионом, страницы без указанных регионов
                        // с указанной или не указанной культурой

                        // прямое соответствие культуры и региона
                        var chosen = g
                            .Items
                            .Where(x => x.CultureToken == _cultureKey && x.Regions.Count > 0)
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
                            .Where(x => x.CultureToken == null && x.Regions.Count > 0)
                            .FirstOrDefault();
                        }

                        // если не указан регион и не указана культура
                        if (chosen == null)
                        {
                            chosen = g.Items
                                .Where(x => x.CultureToken == null && x.Regions.Count == 0)
                                .FirstOrDefault();
                        }

                        // берем первый попавшийся из подошедших
                        if (chosen == null)
                        {
                            chosen = g.Items.FirstOrDefault();
                            Debug.WriteLine("Две одинаковые страницы. " + chosen.Id);
                        }

                        filtered.RemoveAll(x => g.Items.Contains(x) && x != chosen);
                        continue;
                    }
                }
            }

            return filtered;
        }
    }
}
