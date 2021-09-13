using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Mirage.Weaver
{

    internal class PostProcessorReflectionImporter : DefaultReflectionImporter
    {
        private const string SystemPrivateCoreLib = "System.Private.CoreLib";
        private readonly AssemblyNameReference _correctCorlib;

        public PostProcessorReflectionImporter(ModuleDefinition module) : base(module)
        {
            _correctCorlib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib" || a.Name == "netstandard" || a.Name == SystemPrivateCoreLib);
        }

        public override AssemblyNameReference ImportReference(AssemblyName name)
        {
            CheckName(name);
            if (_correctCorlib != null && name.Name == SystemPrivateCoreLib)
            {
                return _correctCorlib;
            }

            if (TryImportFast(name, out AssemblyNameReference reference))
            {
                return reference;
            }

            return base.ImportReference(name);
        }

        static void CheckName(object name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
        }

        bool TryImportFast(AssemblyName name, out AssemblyNameReference assembly_reference)
        {
            // getting full name is expensive
            // we cant cache it because the AssemblyName object might be different each time
            // we can get it once before the loop instead of inside the loop like in DefaultImporter:
            // https://github.com/jbevain/cecil/blob/0.10/Mono.Cecil/Import.cs#L335
            string fullName = name.FullName;

            Collection<AssemblyNameReference> references = module.AssemblyReferences;
            for (int i = 0; i < references.Count; i++)
            {
                AssemblyNameReference reference = references[i];
                if (fullName == reference.FullName)
                {
                    assembly_reference = reference;
                    return true;
                }
            }

            assembly_reference = null;
            return false;
        }
    }
}
