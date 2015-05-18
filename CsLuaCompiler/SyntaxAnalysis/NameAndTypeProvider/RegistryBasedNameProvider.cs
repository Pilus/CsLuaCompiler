﻿namespace CsLuaCompiler.SyntaxAnalysis.NameAndTypeProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using System.IO;

    public class RegistryBasedNameProvider : ITypeProvider
    {
        private readonly LoadedNamespace rootNamespace;
        private List<LoadedNamespace> refenrecedNamespaces;

        public RegistryBasedNameProvider(Solution solution)
        {
            this.rootNamespace = new LoadedNamespace(null);
            this.LoadSystemTypes();
            this.LoadSolution(solution);
        }

        private void LoadSystemTypes()
        {
            this.LoadType(typeof(Action));
            this.LoadType(typeof(Func<int>));
        }

        private void LoadSolution(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                try {
                    this.LoadAssembly(Assembly.LoadFrom(project.OutputFilePath));
                }
                catch (FileNotFoundException)
                {
                    throw new CompilerException(string.Format("Could not find the file {0}. Please build or rebuild the {1} project.", project.OutputFilePath, project.Name));
                }
            }
        }

        private void LoadAssembly(Assembly assembly)
        {
            foreach(var type in assembly.GetTypes().Where(t => !t.Name.StartsWith("<")))
            {
                this.LoadType(type);
            }
        }

        private void LoadType(Type type)
        {
            var nameParts = type.FullName.Split('.');

            if (nameParts.Length < 2)
            {
                throw new NameProviderException(string.Format("Type name does not have any namespace: {0}", type.FullName));
            }

            LoadedNamespace currentNamespace = null;
            foreach (var namePart in nameParts.Take(nameParts.Length - 1))
            {
                currentNamespace = (currentNamespace ?? this.rootNamespace).Upsert(namePart);
            }

            if (currentNamespace == null)
            {
                throw new NameProviderException("No namespace found.");
            }
            currentNamespace.Upsert(type);
        }

        public void SetNamespaces(string currentNamespace, IEnumerable<string> namespaces)
        {
            var baseRefencedNamespaces = new List<LoadedNamespace>()
            {
                this.rootNamespace,
            };

            var currentNamespaceNames = currentNamespace.Split('.');
            for (var i = 1; i <= currentNamespaceNames.Count(); i++)
            {
                baseRefencedNamespaces.Add(this.rootNamespace.TryGetNamespace(currentNamespaceNames.Take(i).ToList()));
            }

            this.refenrecedNamespaces = baseRefencedNamespaces.Where(x => true).ToList();

            foreach (var ns in namespaces)
            {
                var found = false;
                foreach (var refenrecedNamespace in baseRefencedNamespaces)
                {
                    var loadedNamespace = refenrecedNamespace.TryGetNamespace(ns.Split('.').ToList());
                    if (loadedNamespace != null)
                    {
                        this.refenrecedNamespaces.Add(loadedNamespace);
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    throw new NameProviderException(String.Format("Could not find namespace: {0}.", ns));
                }
            }
        }

        

        public string LookupStaticVariableName(IEnumerable<string> names)
        {
            var firstName = names.First();
            
            foreach (var ns in this.refenrecedNamespaces)
            {
                if (ns.Types.ContainsKey(firstName))
                {
                    var type = ns.Types[firstName];
                    var additionalName = string.Empty;
                    if (names.Count() > 1)
                    {
                        additionalName = "." + string.Join(".", names.Skip(1).ToArray());
                    }
                    return type.Type.FullName + additionalName;
                }
            }

            throw new NameProviderException(string.Format("Could not find a variable for {0}", firstName));
        }

        public TypeResult LookupType(string name)
        {
            var nameWithoutGenerics = StripGenerics(name);
            foreach (var refenrecedNamespace in this.refenrecedNamespaces)
            {
                if (refenrecedNamespace.Types.ContainsKey(nameWithoutGenerics))
                {
                    return refenrecedNamespace.Types[nameWithoutGenerics].GetTypeResult();
                }
            }
            throw new NameProviderException(string.Format("Could not find type '{0}' in the referenced namespaces.", name));
        }

        public TypeResult LookupType(IEnumerable<string> names)
        {
            var nameWithoutGenerics = StripGenerics(names.First());
            foreach (var refenrecedNamespace in this.refenrecedNamespaces)
            {
                if (refenrecedNamespace.Types.ContainsKey(nameWithoutGenerics))
                {
                    return refenrecedNamespace.Types[nameWithoutGenerics].GetTypeResult();
                }
                
                if (names.Count() > 1 && refenrecedNamespace.SubNamespaces.ContainsKey(nameWithoutGenerics))
                {
                    var current = refenrecedNamespace.SubNamespaces[nameWithoutGenerics];
                    foreach (var name in names.Skip(1).Select(StripGenerics))
                    {
                        if (current.Types.ContainsKey(name))
                        {
                            return current.Types[name].GetTypeResult();
                        }
                        else if (current.SubNamespaces.ContainsKey(name))
                        {
                            current = current.SubNamespaces[name];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
          
            throw new NameProviderException(string.Format("Could not find type '{0}' in the referenced namespaces.", string.Join(".", names)));
        }


        private static string StripGenerics(string name)
        {
            return name.Split('`').First();
        }


        

    }
}