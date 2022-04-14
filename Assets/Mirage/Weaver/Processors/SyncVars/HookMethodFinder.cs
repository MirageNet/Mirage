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

            SyncHookType hookType = syncVarAttr.GetField<SyncHookType>(nameof(SyncVarAttribute.hookType), SyncHookType.Automatic);

            SyncVarHook hook = FindHookMethod(syncVar, hookFunctionName, hookType, originalType);
            if (hook != null)
                return hook;
            else
                throw new HookMethodException($"Could not find hook for '{syncVar.Name}', hook name '{hookFunctionName}', hook type {hookType}. See SyncHookType for valid signatures", syncVar);
        }

        static SyncVarHook FindHookMethod(FieldDefinition syncVar, string hookFunctionName, SyncHookType hookType, TypeReference originalType)
        {
            switch (hookType)
            {
                default:
                case SyncHookType.Automatic:
                    return FindAutomatic(syncVar, hookFunctionName, originalType);
                case SyncHookType.MethodWith1Arg:
                    return FindMethod1Arg(syncVar, hookFunctionName, originalType);
                case SyncHookType.MethodWith2Arg:
                    return FindMethod2Arg(syncVar, hookFunctionName, originalType);
                case SyncHookType.EventWith1Arg:
                    return FindEvent1Arg(syncVar, hookFunctionName, originalType);
                case SyncHookType.EventWith2Arg:
                    return FindEvent2Arg(syncVar, hookFunctionName, originalType);
            }
        }

        private static SyncVarHook FindAutomatic(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            SyncVarHook foundHook = null;

            CheckHook(syncVar, hookFunctionName, ref foundHook, FindMethod1Arg(syncVar, hookFunctionName, originalType));
            CheckHook(syncVar, hookFunctionName, ref foundHook, FindMethod2Arg(syncVar, hookFunctionName, originalType));
            CheckHook(syncVar, hookFunctionName, ref foundHook, FindEvent1Arg(syncVar, hookFunctionName, originalType));
            CheckHook(syncVar, hookFunctionName, ref foundHook, FindEvent2Arg(syncVar, hookFunctionName, originalType));

            return foundHook;
        }

        static void CheckHook(FieldDefinition syncVar, string hookFunctionName, ref SyncVarHook foundHook, SyncVarHook newfound)
        {
            // dont need to check anything if new one is null (not found)
            if (newfound == null)
                return;

            if (foundHook == null)
            {
                foundHook = newfound;
            }
            else
            {
                throw new HookMethodException($"Mutliple hooks found for '{syncVar.Name}', hook name '{hookFunctionName}'. Please set HookType or remove one of the overloads", syncVar);
            }
        }

        private static SyncVarHook FindMethod1Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            MethodDefinition[] methods = syncVar.DeclaringType.GetMethods(hookFunctionName);
            MethodDefinition[] methodsWith1Param = methods.Where(m => m.Parameters.Count == 1).ToArray();
            if (methodsWith1Param.Length == 0)
            {
                return null;
            }

            // return method if matching args are found
            foreach (MethodDefinition method in methodsWith1Param)
            {
                if (MatchesParameters(method, originalType, 1))
                {
                    return new SyncVarHook { Method = method };
                }
            }

            // else throw saying args were wrong
            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'. ", syncVar);
        }

        private static SyncVarHook FindMethod2Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            MethodDefinition[] methods = syncVar.DeclaringType.GetMethods(hookFunctionName);
            MethodDefinition[] methodsWith2Param = methods.Where(m => m.Parameters.Count == 2).ToArray();

            if (methodsWith2Param.Length == 0)
            {
                return null;
            }

            // return method if matching args are found
            foreach (MethodDefinition method in methodsWith2Param)
            {
                if (MatchesParameters(method, originalType, 2))
                {
                    return new SyncVarHook { Method = method };
                }
            }

            // else throw saying args were wrong
            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'. ", syncVar);
        }

        private static SyncVarHook FindEvent1Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            // we can't have 2 events/fields with same name, so using `First` is ok here
            EventDefinition @event = syncVar.DeclaringType.Events.FirstOrDefault(x => x.Name == hookFunctionName);
            if (@event == null)
                return null;

            return ValidateEvent(syncVar, originalType, @event, 1);
        }

        private static SyncVarHook FindEvent2Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            // we can't have 2 events/fields with same name, so using `First` is ok here
            EventDefinition @event = syncVar.DeclaringType.Events.FirstOrDefault(x => x.Name == hookFunctionName);
            if (@event == null)
                return null;

            return ValidateEvent(syncVar, originalType, @event, 2);
        }

        private static SyncVarHook ValidateEvent(FieldDefinition syncVar, TypeReference originalType, EventDefinition @event, int argCount)
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
            if (args.Count != argCount)
            {
                // ok to not have matching count
                // we could be hookType.Automatic and looking for 1 arg, when there is event with 2 args
                return null;
            }

            if (MatchesParameters(genericEvent, originalType, argCount))
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            return new SyncVarHook { Event = @event };
        }

        private static void ThrowWrongHookType(FieldDefinition syncVar, EventDefinition @event, TypeReference eventType)
        {
            throw new HookMethodException($"Hook Event for '{syncVar.Name}' needs to be type 'System.Action<,>' but was '{eventType.FullName}' instead", @event);
        }

        static bool MatchesParameters(GenericInstanceType genericEvent, TypeReference originalType, int count)
        {
            // matches event Action<T, T> eventName;
            Collection<TypeReference> args = genericEvent.GenericArguments;
            for (int i = 0; i < count; i++)
            {
                if (args[i].FullName != originalType.FullName)
                    return false;
            }
            return true;
        }
        static bool MatchesParameters(MethodDefinition method, TypeReference originalType, int count)
        {
            // matches void onValueChange(T oldValue, T newValue)
            Collection<ParameterDefinition> parameters = method.Parameters;
            for (int i = 0; i < count; i++)
            {
                if (parameters[i].ParameterType.FullName != originalType.FullName)
                    return false;
            }
            return true;
        }
    }
}
