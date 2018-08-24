using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using QA.Core.Engine.Editing.Models;
using QA.Core.Engine.UI;
using QA.Core.Engine.Web;
using QA.Core.Engine.Web.Mvc;
#pragma warning disable 1591

namespace QA.Core.Engine.Editing.Controllers
{
    /// <summary>
    /// Контроллер для управлением виджетами
    /// </summary>
    [Permitions]
    [Content]
    public class WidgetManagmentController : QA.Core.Web.QAAsyncControllerBase
    {
        private const string ViewRoot = "~/App_Resource/QA.Core.Engine.dll/Editing/Views/WidgetManagment/";
        private IAbstractItemFieldNameResolver _fieldNameResolver;

        public WidgetManagmentController()
            : this(new AbstractItemFieldNameResolver())
        {
        }

        public WidgetManagmentController(IAbstractItemFieldNameResolver fieldNameResolver)
        {
            _fieldNameResolver = fieldNameResolver;
        }

        /// <summary>
        /// Получение данный для создания виджета
        /// <remarks> path: path, zone: zone, type: type, itemId: itemId</remarks>
        /// </summary>
        /// <returns></returns>
        public JsonResult OnCreatePart(string path, string zone, string type, string itemId)
        {
            // TODO: проверить права пользователя
            IDefinitionManager manager = ObjectFactoryBase.Resolve<IDefinitionManager>();

            var definitions = manager.GetDefinitions().ToList();

            if (!definitions.Any(x => x.Discriminator.Equals(type, StringComparison.InvariantCultureIgnoreCase)))
            {
                return JsonError(null, "Для данного типа виджета нет описания.", JsonRequestBehavior.AllowGet);
            }

            var def = definitions.FirstOrDefault(x => x.Discriminator.Equals(type, StringComparison.InvariantCultureIgnoreCase));

            if (def.IsPage)
            {
                return JsonError(null, "На страницу можно добавлять только виджеты.", JsonRequestBehavior.AllowGet);
            }

            int parentId = 0;
            if (!int.TryParse(path, out parentId))
            {
                return JsonError(null, "Формат родительского элемента указан неверно.", JsonRequestBehavior.AllowGet);
            }

            if (zone.ToLower().StartsWith("site"))
            {
                using (var persister = ObjectFactoryBase.Resolve<IPersister>())
                {
                    var destination = RouteExtensions.ResolveService<IUrlParser>(RouteData).StartPage;
                    parentId = destination.Id;

                }
            }

            return JsonSuccess(new
            {
                FieldsToDisable = new[]
                {
                    _fieldNameResolver.Resolve("Parent"),
                    _fieldNameResolver.Resolve("IsPage"),
                    _fieldNameResolver.Resolve("Discriminator"),
                    _fieldNameResolver.Resolve("VersionOf"),
                    _fieldNameResolver.Resolve("ExtensionId"),
                },
                FieldsToSet = new[]
                {
                    new FieldToSetModel { fieldName = _fieldNameResolver.Resolve("Parent"), value = parentId },
                    new FieldToSetModel { fieldName = _fieldNameResolver.Resolve("ZoneName"), value = zone },
                    new FieldToSetModel { fieldName = _fieldNameResolver.Resolve("Discriminator"), value = def.Id },
                    new FieldToSetModel { fieldName = _fieldNameResolver.Resolve("ExtensionId"), value = def.PreferredContentId },
                    new FieldToSetModel { fieldName = _fieldNameResolver.Resolve("IsPage"), value = false },
                },
                FieldsToHide = new[]
                {
                    _fieldNameResolver.Resolve("IsInSiteMap"),
                }
            }, "OK", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MoveNode(MoveNodeModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO:
                // проверить права
                // проверить возможность перемещения элементов
                // переместить

                if (model.MovementType == "before" || model.MovementType == "after")
                {
                    return JsonError(false, "Reordering items is not supported yet.");
                }

                // movement type is "last".

                using (var persister = ObjectFactoryBase.Resolve<IPersister>())
                {

                    var source = persister.Get(model.TargetNodeId);
                    AbstractItem destination = null;

                    if (model.ZoneName.ToLower().StartsWith("site"))
                    {
                        destination = RouteExtensions.ResolveService<IUrlParser>(RouteData).StartPage;
                    }
                    else
                    {
                        destination = persister.Get(model.DestinationNodeId);
                    }

                    if (source == null || destination == null)
                    {
                        return JsonError(null, "Item with such id is not found.");
                    }

                    if (!CheckUrl(source, destination))
                    {
                        return JsonError(null, "Source node contains target node.");
                    }
                    if (ObjectFactoryBase.Resolve<IUrlParser>()
                        .IsStartPage(source))
                    {
                        return JsonError(null, "Manipulations with the start page are not allowed.");
                    }

                    source.Parent = destination;
                    source.ZoneName = model.ZoneName;

                    persister.Save(source, false);
                    persister.Flush();

                    try
                    {
                        // TODO: retrieve actual Urtl
                        AbstractItem currentItem = persister.Get(model.TargetNodeId);

                        var url = new Url(currentItem.Url);

                        url = url.RemoveQuery(ContentAttribute.UrlQueryStringKey);
                        url = url.AppendQuery(ContentAttribute.UrlQueryStringKey, currentItem.Url);
                        return JsonSuccess(new { Url = url.ToString() }, null);
                    }
                    catch (Exception ex)
                    {
                        return JsonError(null, ex.Message);
                    }
                }
            }
            else
            {
                return JsonValidation();
            }
        }

        public ActionResult PreviewItem(int? itemId, int? chosenRegion, string cultureName)
        {
            if (itemId != null)
            {

                using (var persister = ObjectFactoryBase.Resolve<IPersister>())
                {
                    var item = persister.Get(itemId.Value);
                    if (item != null)
                    {
                        var parser = ObjectFactoryBase.Resolve<IUrlParser>();

                        var region = item.Regions
                            .Where(x => x.Id == chosenRegion)
                            .Select(x => x.Alias)
                            .FirstOrDefault();

                        var culture = !string.IsNullOrEmpty(cultureName) ? cultureName : (item.Culture != null ? item.Culture.Key : string.Empty);

                        var url = parser.BuildUrl(item, region, culture);

                        // при перенаправлении теряется парметр hostuid, что делает невозможным onscreen
                        var legacyUrl = new Url(url);

                        // TODO: remove magic keys!
                        var hostKey = ConfigurationManager.AppSettings["QP.HostIdParamName"] ?? "hostUID";

                        var hostId = Request.QueryString[hostKey];

                        if (!string.IsNullOrEmpty(hostId))
                        {
                            legacyUrl = legacyUrl.AppendQuery(hostKey, hostId);
                        }

                        // сделать выбор корретного dns

                        AbstractItem node = item;

                        var authority = new Url(Request.Url.ToString()).Authority;

                        if (!string.IsNullOrEmpty(authority))
                        {
                            while (node != null)
                            {
                                if (node is IStartPage)
                                {
                                    var startPage = (IStartPage)node;

                                    var bindings = startPage.GetDNSBindings();

                                    if (bindings.Length > 0 &&
                                        !bindings.Any(x => authority.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        legacyUrl = legacyUrl.SetAuthority(bindings[0]);
                                    }

                                    break;
                                }

                                node = node.Parent;
                            }
                        }

                        return Redirect(legacyUrl);
                    }
                }
            }

            return Content("An item with such itemId is not found or not presented in requested region.");
        }


        private static bool CheckUrl(AbstractItem target, AbstractItem destination)
        {
            // HARDCODE DETECTED!!!!
            // TODO: use trails
            var trail1 = target.GetTrail();
            var trail2 = destination.GetTrail();
            return !trail2
                .Contains(trail1);
        }
    }
}
