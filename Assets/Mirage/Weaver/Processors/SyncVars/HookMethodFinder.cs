using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal static class HookMethodFinder
    {
        /// <returns>Found Hook method or null</returns>
        /// <exception cref="HookMethodException">Throws if users sets hook in attribute but method could not be found</exception>
        public static MethodDefinition GetHookMethod(FieldDefinition syncVar, TypeReference originalType)
        {
            CustomAttribute syncVarAttr = syncVar.GetCustomAttribute<SyncVarAttribute>();

            if (syncVarAttr == null)
                throw new InvalidOperationException("FoundSyncVar did not have a SyncVarAttribute");

            string hookFunctionName = syncVarAttr.GetField<string>("hook", null);

            if (string.IsNullOrEmpty(hookFunctionName))
                return null;

            return FindHookMethod(syncVar, hookFunctionName, originalType);
        }

        static MethodDefinition FindHookMethod(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            List<MethodDefinition> methods = syncVar.DeclaringType.GetMethods(hookFunctionName);

            var methodsWith2Param = new List<MethodDefinition>(methods.Where(m => m.Parameters.Count == 2));

            if (methodsWith2Param.Count == 0)
            {
                throw new HookMethodException($"Could not find hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                    $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                    syncVar);
            }

            foreach (MethodDefinition method in methodsWith2Param)
            {
                if (MatchesParameters(method, originalType))
                {
                    return method;
                }
            }

            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                syncVar);
        }

        static string HookParameterMessage(string hookName, TypeReference ValueType)
            => string.Format("void {0}({1} oldValue, {1} newValue)", hookName, ValueType);


        static bool MatchesParameters(MethodDefinition method, TypeReference originalType)
        {
            // matches void onValueChange(T oldValue, T newValue)
            return method.Parameters[0].ParameterType.FullName == originalType.FullName &&
                   method.Parameters[1].ParameterType.FullName == originalType.FullName;
        }

    }
}
