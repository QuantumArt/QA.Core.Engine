// Owners: Karlov Nikolay

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using QA.Core.Engine.UI;

namespace QA.Core.Engine
{
    public class ControllerMapper : IControllerMapper
    {
        private readonly IDictionary<Type, string> _controllerMap = new Dictionary<Type, string>();
        private readonly IDictionary<string, string[]> _controllerActionMap = new Dictionary<string, string[]>();

        public ControllerMapper(ITypeFinder typeFinder, IDefinitionManager definitionManager)
        {
            IList<ControlsAttribute> controllerDefinitions = FindControllers(typeFinder);
            foreach (ItemDefinition id in definitionManager.GetDefinitions())
            {
                IAdapterDescriptor controllerDefinition = GetControllerFor(id.ItemType, controllerDefinitions);
                if (controllerDefinition != null)
                {
                    ControllerMap[id.ItemType] = controllerDefinition.ControllerName;
                    foreach (var finder in PathDictionary.GetFinders(id.ItemType).Where(f => f is ActionResolver))
                        PathDictionary.RemoveFinder(id.ItemType, finder);

                    var methods = new ReflectedControllerDescriptor(controllerDefinition.AdapterType)
                        .GetCanonicalActions()
                        .Select(m => m.ActionName).ToArray();

                    var actionResolver = new ActionResolver(this, methods);

                    _controllerActionMap[controllerDefinition.ControllerName] = methods;

                    // если включен LowercaseRoute
                    _controllerActionMap[controllerDefinition.ControllerName.ToLower()] = methods;

                    PathDictionary.PrependFinder(id.ItemType, actionResolver);
                }
            }
        }

        public string GetControllerName(Type type)
        {
            string name;
            ControllerMap.TryGetValue(type, out name);
            return name;
        }

        public bool ControllerHasAction(string controllerName, string actionName)
        {
            if (!ControllerActionMap.ContainsKey(controllerName))
                return false;

            foreach (var action in ControllerActionMap[controllerName])
            {
                if (String.Equals(action, actionName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }


        public IEnumerable<string> GetActionsFor<T>()
            where T : AbstractItem
        {
            return GetActionsForItemType(typeof(T));
        }

        public IEnumerable<string> GetActionsForItemType(Type itemType)
        {
            string controller;
            if (ControllerMap.TryGetValue(itemType, out controller))
            {
                string[] actions;
                if (ControllerActionMap.TryGetValue(controller, out actions))
                {
                    return actions;
                }
            }
            return new string[] { };
        }

        protected IDictionary<Type, string> ControllerMap
        {
            get { return _controllerMap; }
        }

        protected IDictionary<string, string[]> ControllerActionMap
        {
            get
            {
                return _controllerActionMap;
            }
        }

        private static IAdapterDescriptor GetControllerFor(Type itemType, IList<ControlsAttribute> controllerDefinitions)
        {
            foreach (ControlsAttribute controllerDefinition in controllerDefinitions)
            {
                if (controllerDefinition.ItemType.IsAssignableFrom(itemType))
                {
                    return controllerDefinition;
                }
            }
            return null;
        }

        private static IList<ControlsAttribute> FindControllers(ITypeFinder typeFinder)
        {
            var controllerLookup = new Dictionary<Type, ControlsAttribute>();
            foreach (Type controllerType in typeFinder.Find(typeof(IController)))
            {
                foreach (ControlsAttribute attr in controllerType.GetCustomAttributes(typeof(ControlsAttribute), false))
                {
                    if (controllerLookup.ContainsKey(attr.ItemType))
                        throw new Exception("Duplicate controller " + controllerType.Name + " declared for item type " +
                                              attr.ItemType.Name +
                                              " The controller " + controllerLookup[attr.ItemType].AdapterType.Name +
                                              " already handles this type and two controllers cannot handle the same item type.");

                    attr.AdapterType = controllerType;
                    controllerLookup.Add(attr.ItemType, attr);
                }
            }

            var controllerDefinitions = new List<ControlsAttribute>(controllerLookup.Values);
            controllerDefinitions.Sort();

            return controllerDefinitions;
        }
    }

    [DebuggerDisplay("ControlsAttribute: {AdapterType}->{ItemType}")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class ControlsAttribute : Attribute, IComparable<ControlsAttribute>, IAdapterDescriptor
    {
        private readonly Type itemType;
        private Type adapterType;

        public ControlsAttribute(Type itemType)
        {
            this.itemType = itemType;
        }


        public Type ItemType
        {
            get { return itemType; }
        }


        public Type AdapterType
        {
            get { return adapterType; }
            set { adapterType = value; }
        }


        public string ControllerName
        {
            get
            {
                string name = AdapterType.Name;
                int i = name.IndexOf("Controller");
                if (i > 0)
                {
                    return name.Substring(0, i);
                }
                return name;
            }
        }

        public bool IsAdapterFor(PathData path, Type requiredType)
        {
            if (path.IsEmpty())
                return false;

            return ItemType.IsAssignableFrom(path.CurrentItem.GetContentType()) && requiredType.IsAssignableFrom(adapterType);
        }

        #region IComparable<IAdapterDescriptor> Members

        public int CompareTo(IAdapterDescriptor other)
        {
            return InheritanceDepth(other.ItemType) - InheritanceDepth(ItemType);
        }

        #endregion

        #region IComparable<ControlsAttribute> Members

        int IComparable<ControlsAttribute>.CompareTo(ControlsAttribute other)
        {
            return CompareTo(other);
        }

        #endregion

        public static int InheritanceDepth(Type type)
        {
            if (type == null)
                return -1;
            if (type == typeof(object))
                return 0;

            return Math.Max(
                1 + InheritanceDepth(type.BaseType),
                type.GetInterfaces().Select(t => 1 + InheritanceDepth(t)).OrderByDescending(d => d).FirstOrDefault());
        }
    }

    public interface IAdapterDescriptor : IComparable<IAdapterDescriptor>
    {
        Type ItemType { get; }
        Type AdapterType { get; set; }

        bool IsAdapterFor(PathData path, Type requiredType);

        string ControllerName { get; }
    }

    public interface ITypeFinder
    {
        /// <summary>
        /// Регистрировать сборку, в которой описаны классы с виджетами и страницами и контроллеры в случае,
        /// если данные типи описаны не в стартовом проекте.
        /// </summary>
        ITypeFinder RegisterAssemblyWithType(Type typeExportedByAsseblyToRegister);
        IList<Type> Find(Type requestedType);
        IList<Assembly> GetAssemblies();
    }
}
