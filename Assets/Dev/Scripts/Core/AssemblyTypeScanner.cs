using System;
using System.Collections.Generic;
using System.Reflection;

namespace PackNFlow.Core
{
    public static class AssemblyTypeScanner
    {
        private enum RuntimeAssembly
        {
            Main,
            Editor,
            EditorFirstPass,
            FirstPass
        }

        private static RuntimeAssembly? ClassifyAssembly(string assemblyName)
        {
            return assemblyName switch
            {
                "Assembly-CSharp" => RuntimeAssembly.Main,
                "Assembly-CSharp-Editor" => RuntimeAssembly.Editor,
                "Assembly-CSharp-Editor-firstpass" => RuntimeAssembly.EditorFirstPass,
                "Assembly-CSharp-firstpass" => RuntimeAssembly.FirstPass,
                _ => null
            };
        }

        private static void CollectMatchingTypes(Type[] sourceTypes, Type interfaceType, ICollection<Type> results)
        {
            if (sourceTypes == null) return;

            for (int i = 0; i < sourceTypes.Length; i++)
            {
                Type t = sourceTypes[i];
                if (t != interfaceType && interfaceType.IsAssignableFrom(t))
                {
                    results.Add(t);
                }
            }
        }

        public static List<Type> GetTypes(Type interfaceType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var classifiedTypes = new Dictionary<RuntimeAssembly, Type[]>();
            var results = new List<Type>();

            for (int i = 0; i < assemblies.Length; i++)
            {
                RuntimeAssembly? classification = ClassifyAssembly(assemblies[i].GetName().Name);
                if (classification.HasValue)
                {
                    classifiedTypes.Add(classification.Value, assemblies[i].GetTypes());
                }
            }

            if (classifiedTypes.TryGetValue(RuntimeAssembly.Main, out var mainTypes))
                CollectMatchingTypes(mainTypes, interfaceType, results);

            if (classifiedTypes.TryGetValue(RuntimeAssembly.FirstPass, out var firstPassTypes))
                CollectMatchingTypes(firstPassTypes, interfaceType, results);

            return results;
        }
    }
}
