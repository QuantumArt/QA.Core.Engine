using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace QA.Core.Engine.Data
{
    /// <summary>
    /// AppDomain Type Finder
    /// </summary>
    public class FakeTypeFinder : ITypeFinder
    {
        #region Private Fields

        private bool loadAppDomainAssemblies = true;

        private string assemblySkipLoadingPattern = "^QA.Core.Data|^System|^mscorlib|^Microsoft|^EntityFramework|^CppCodeProvider|^VJSharpCodeProvider|^WebDev|^nlog|^Iesi|^log4net|^MbUnit|^Rhino|^QuickGraph|^TestFu|^Telerik|^ComponentArt|^MvcContrib|^AjaxControlToolkit|^Antlr3|^Remotion|^Recaptcha|^Lucene|^Ionic|^Spark|^SharpArch|^CommonServiceLocator|^Newtonsoft|^SMDiagnostics|^App_LocalResources|^AntiXSSLibrary|^HtmlSanitizationLibrary|^sqlce|^WindowsBase|^DynamicProxyGenAssembly|^Anonymously Hosted DynamicMethods Assembly";

        private string assemblyRestrictToLoadingPattern = ".*";
        private IList<string> assemblyNames = new List<string>();

        #endregion

        #region Properties

        public virtual AppDomain App
        {
            get { return AppDomain.CurrentDomain; }
        }

        public bool LoadAppDomainAssemblies
        {
            get { return loadAppDomainAssemblies; }
            set { loadAppDomainAssemblies = value; }
        }

        public IList<string> AssemblyNames
        {
            get { return assemblyNames; }
            set { assemblyNames = value; }
        }

        public string AssemblySkipLoadingPattern
        {
            get { return assemblySkipLoadingPattern; }
            set { assemblySkipLoadingPattern = value; }
        }

        public string AssemblyRestrictToLoadingPattern
        {
            get { return assemblyRestrictToLoadingPattern; }
            set { assemblyRestrictToLoadingPattern = value; }
        }

        #endregion

        public virtual IList<Type> Find(Type requestedType)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly a in GetAssemblies())
            {
                try
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (requestedType.IsAssignableFrom(t))
                        {
                            types.Add(t);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    string loaderErrors = string.Empty;
                    foreach (Exception loaderEx in ex.LoaderExceptions)
                    {
                        Trace.TraceError(loaderEx.ToString());
                        loaderErrors += ", " + loaderEx.Message;
                    }

                    throw new Exception("Error getting types from assembly " + a.FullName + loaderErrors, ex);
                }
            }

            return types;
        }

        public virtual IList<Assembly> GetAssemblies()
        {
            List<string> addedAssemblyNames = new List<string>();
            List<Assembly> assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies)
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
            AddConfiguredAssemblies(addedAssemblyNames, assemblies);

            return assemblies;
        }

        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (Assembly assembly in App.GetAssemblies())
            {
                if (Matches(assembly.FullName))
                {
                    if (!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                        addedAssemblyNames.Add(assembly.FullName);
                    }
                }
            }
        }

        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (string assemblyName in AssemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                if (!addedAssemblyNames.Contains(assembly.FullName))
                {
                    assemblies.Add(assembly);
                    addedAssemblyNames.Add(assembly.FullName);
                }
            }
        }

        public virtual bool Matches(string assemblyFullName)
        {
            return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
                   && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
        }

        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            List<string> loadedAssemblyNames = new List<string>();
            foreach (Assembly a in GetAssemblies())
            {
                loadedAssemblyNames.Add(a.FullName);
            }

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (string dllPath in Directory.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    string assumedAssemblyName = Path.GetFileNameWithoutExtension(dllPath);
                    if (Matches(assumedAssemblyName) && !loadedAssemblyNames.Contains(assumedAssemblyName))
                    {
                        App.Load(assumedAssemblyName);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }
    }
}
