﻿namespace CsLuaConverter.Providers.TypeProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using TypeKnowledgeRegistry;

    public class LoadedNamespace
    {
        public LoadedNamespace(string name)
        {
            this.Name = name;
        }

        public LoadedNamespace Upsert(string nameSpaceName)
        {
            if (!this.SubNamespaces.ContainsKey(nameSpaceName))
            {
                this.SubNamespaces.Add(nameSpaceName, new LoadedNamespace(nameSpaceName));
            }
            return this.SubNamespaces[nameSpaceName];
        }

        public void Upsert(Type type)
        {
            var name = StripGenerics(type.Name);
            if (!this.Types.ContainsKey(name))
            {
                this.Types[name] = new List<LoadedType>();
            }

            this.Types[name].Add(new LoadedType(type));
            this.AddPossibleExtensionMethods(type);
        }

        private static string StripGenerics(string name)
        {
            return name.Split('`').First();
        }

        public LoadedNamespace TryGetNamespace(IList<string> names)
        {
            var name = StripGenerics(names.First());
            if (this.SubNamespaces.ContainsKey(name))
            {
                return names.Count > 1 ? this.SubNamespaces[name].TryGetNamespace(names.Skip(1).ToList()) : this.SubNamespaces[name];
            }
            return null;
        }

        public TypeKnowledge[] GetExtensionMethods(Type type, string name)
        {
            return !this.ExtensionMethods.ContainsKey(name) ? new TypeKnowledge[] {} :
            this.ExtensionMethods[name].Where(extension =>
            {
                throw new NotImplementedException();
            }).Select(v => v.Item2).ToArray();
        }

        private void AddPossibleExtensionMethods(Type type)
        {
            if (!IsExtensionClass(type))
            {
                return;
            }

            var extensionMethods = type.GetMethods().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof (ExtensionAttribute)));

            foreach (var method in extensionMethods)
            {
                var extensionType = method.GetParameters().First().ParameterType;
                this.AddExtensionMethod(method.Name, extensionType, new TypeKnowledge(method, true));
            }
        }

        private static bool IsExtensionClass(Type type)
        {
            return type.CustomAttributes.Any(a => a.AttributeType == typeof (ExtensionAttribute));
        }

        public string Name;
        public Dictionary<string, LoadedNamespace> SubNamespaces = new Dictionary<string, LoadedNamespace>();
        public Dictionary<string, IList<LoadedType>> Types = new Dictionary<string, IList<LoadedType>>();

        private Dictionary<string, IList<Tuple<Type, TypeKnowledge>>> ExtensionMethods = new Dictionary<string, IList<Tuple<Type, TypeKnowledge>>>();

        private void AddExtensionMethod(string name, Type extensionType, TypeKnowledge typeKnowledgeForMethod)
        {
            if (!this.ExtensionMethods.ContainsKey(name))
            {
                this.ExtensionMethods.Add(name, new List<Tuple<Type, TypeKnowledge>>());
            }

            this.ExtensionMethods[name].Add(new Tuple<Type, TypeKnowledge>(extensionType, typeKnowledgeForMethod));
        }
    }
}