// all the resolve functions for the weaver
// NOTE: these functions should be made extensions, but right now they still
//       make heavy use of Weaver.fail and we'd have to check each one's return
//       value for null otherwise.
//       (original FieldType.Resolve returns null if not found too, so
//        exceptions would be a bit inconsistent here)
using Mono.Cecil;

namespace Mirage.Weaver
{
    public static class Resolvers
    {
        public static MethodDefinition ResolveDefaultPublicCtor(TypeReference variable)
        {
            TypeDefinition td = variable.Resolve();
            if (td == null)
                return null;

            foreach (MethodDefinition methodRef in td.Methods)
            {
                if (methodRef.IsConstructor &&
                    methodRef.Resolve().IsPublic &&
                    methodRef.Parameters.Count == 0)
                {
                    return methodRef;
                }
            }
            return null;
        }

        public static MethodReference ResolveProperty(TypeReference tr, AssemblyDefinition scriptDef, string name)
        {
            TypeDefinition td = tr.Resolve();
            if (td == null)
                return null;

            foreach (PropertyDefinition pd in td.Properties)
            {
                if (pd.Name == name)
                {
                    return scriptDef.MainModule.ImportReference(pd.GetMethod);
                }
            }
            return null;
        }
    }
}
