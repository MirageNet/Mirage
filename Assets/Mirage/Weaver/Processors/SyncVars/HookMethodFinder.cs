using System;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Mirage.Weaver.SyncVars
{
    class SyncVarHook
    {
        public MethodDefinition Method;
        public EventDefinition Event;
    }
    internal static class HookMethodFinder
    {
        /// <returns>Found Hook method or null</returns>
        /// <exception cref="HookMethodException">Throws if users sets hook in attribute but method could not be found</exception>
        public static SyncVarHook GetHookMethod(FieldDefinition syncVar, TypeReference originalType)
        {
            CustomAttribute syncVarAttr = syncVar.GetCustomAttribute<SyncVarAttribute>();

            if (syncVarAttr == null)
                throw new InvalidOperationException("FoundSyncVar did not have a SyncVarAttribute");

            string hookFunctionName = syncVarAttr.GetField<string>(nameof(SyncVarAttribute.hook), null);

            if (string.IsNullOrEmpty(hookFunctionName))
                return null;

            return FindHookMethod(syncVar, hookFunctionName, originalType);
        }

        static SyncVarHook FindHookMethod(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            // check event first
            EventDefinition @event = syncVar.DeclaringType.Events.FirstOrDefault(x => x.Name == hookFunctionName);
            if (@event != null)
            {
                return ValidateEvent(syncVar, originalType, @event);
            }

            MethodDefinition[] methods = syncVar.DeclaringType.GetMethods(hookFunctionName);
            MethodDefinition[] methodsWith2Param = methods.Where(m => m.Parameters.Count == 2).ToArray();

            if (methodsWith2Param.Length == 0)
            {
                throw new HookMethodException($"Could not find hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                    $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                    syncVar);
            }

            foreach (MethodDefinition method in methodsWith2Param)
            {
                if (MatchesParameters(method, originalType))
                {
                    return new SyncVarHook { Method = method };
                }
            }

            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                syncVar);
        }

        private static SyncVarHook ValidateEvent(FieldDefinition syncVar, TypeReference originalType, EventDefinition @event)
        {
            TypeReference eventType = @event.EventType;
            if (!eventType.FullName.Contains("System.Action"))
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            if (!eventType.IsGenericInstance)
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            var genericEvent = (GenericInstanceType)eventType;
            Collection<TypeReference> args = genericEvent.GenericArguments;
            if (args.Count != 2)
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            if (args[0].FullName != originalType.FullName || args[1].FullName != originalType.FullName)
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            return new SyncVarHook { Event = @event };
        }

        private static void ThrowWrongHookType(FieldDefinition syncVar, EventDefinition @event, TypeReference eventType)
        {
            throw new HookMethodException($"Hook Event for '{syncVar.Name}' needs to be type 'System.Action<,>' but was '{eventType.FullName}' instead", @event);
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
