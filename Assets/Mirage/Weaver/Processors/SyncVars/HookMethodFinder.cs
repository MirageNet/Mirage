using System;
using System.Linq;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal class SyncVarHook
    {
        public readonly MethodDefinition Method;
        public readonly EventDefinition Event;
        public readonly SyncHookType hookType;

        public SyncVarHook(MethodDefinition method, SyncHookType hookType)
        {
            Method = method;
            this.hookType = hookType;
        }
        public SyncVarHook(EventDefinition @event, SyncHookType hookType)
        {
            Event = @event;
            this.hookType = hookType;
        }

    }
    internal static class HookMethodFinder
    {
        /// <returns>Found Hook method or null</returns>
        /// <exception cref="HookMethodException">Throws if users sets hook in attribute but method could not be found</exception>
        public static SyncVarHook GetHookMethod(FieldDefinition syncVar, TypeReference originalType)
        {
            var syncVarAttr = syncVar.GetCustomAttribute<SyncVarAttribute>();

            if (syncVarAttr == null)
                throw new InvalidOperationException("FoundSyncVar did not have a SyncVarAttribute");

            var hookFunctionName = syncVarAttr.GetField<string>(nameof(SyncVarAttribute.hook), null);

            if (string.IsNullOrEmpty(hookFunctionName))
                return null;

            var hookType = syncVarAttr.GetField<SyncHookType>(nameof(SyncVarAttribute.hookType), SyncHookType.Automatic);

            var hook = FindHookMethod(syncVar, hookFunctionName, hookType, originalType);
            if (hook != null)
                return hook;
            else
                throw new HookMethodException($"Could not find hook for '{syncVar.Name}', hook name '{hookFunctionName}', hook type {hookType}. See SyncHookType for valid signatures", syncVar);
        }

        private static SyncVarHook FindHookMethod(FieldDefinition syncVar, string hookFunctionName, SyncHookType hookType, TypeReference originalType)
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

        private static void CheckHook(FieldDefinition syncVar, string hookFunctionName, ref SyncVarHook foundHook, SyncVarHook newfound)
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
            return ValidateMethod(syncVar, hookFunctionName, originalType, 1);
        }

        private static SyncVarHook FindMethod2Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            return ValidateMethod(syncVar, hookFunctionName, originalType, 2);
        }

        private static SyncVarHook ValidateMethod(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType, int argCount)
        {
            var methods = syncVar.DeclaringType.GetMethods(hookFunctionName);
            var methodsWithParams = methods.Where(m => m.Parameters.Count == argCount).ToArray();
            if (methodsWithParams.Length == 0)
            {
                return null;
            }

            // return method if matching args are found
            foreach (var method in methodsWithParams)
            {
                if (MatchesParameters(method, originalType, argCount))
                {
                    return new SyncVarHook(method, argCount == 1 ? SyncHookType.MethodWith1Arg : SyncHookType.MethodWith2Arg);
                }
            }

            // else throw saying args were wrong
            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'.", syncVar, methods.First());
        }

        private static SyncVarHook FindEvent1Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            return ValidateEvent(syncVar, originalType, hookFunctionName, 1);
        }

        private static SyncVarHook FindEvent2Arg(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            return ValidateEvent(syncVar, originalType, hookFunctionName, 2);
        }

        private static SyncVarHook ValidateEvent(FieldDefinition syncVar, TypeReference originalType, string hookFunctionName, int argCount)
        {
            // we can't have 2 events/fields with same name, so using `First` is ok here
            var @event = syncVar.DeclaringType.Events.FirstOrDefault(x => x.Name == hookFunctionName);
            if (@event == null)
                return null;

            var eventType = @event.EventType;
            if (!eventType.FullName.Contains("System.Action"))
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            if (!eventType.IsGenericInstance)
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            var genericEvent = (GenericInstanceType)eventType;
            var args = genericEvent.GenericArguments;
            if (args.Count != argCount)
            {
                // ok to not have matching count
                // we could be hookType.Automatic and looking for 1 arg, when there is event with 2 args
                return null;
            }

            if (MatchesParameters(genericEvent, originalType, argCount))
            {
                return new SyncVarHook(@event, argCount == 1 ? SyncHookType.EventWith1Arg : SyncHookType.EventWith2Arg);
            }
            else
            {
                ThrowWrongHookType(syncVar, @event, eventType);
            }

            throw new InvalidOperationException("Code should never reach even, should return or throw ealier");
        }

        private static void ThrowWrongHookType(FieldDefinition syncVar, EventDefinition @event, TypeReference eventType)
        {
            throw new HookMethodException($"Hook Event for '{syncVar.Name}' needs to be type 'System.Action<,>' but was '{eventType.FullName}' instead", @event);
        }

        private static bool MatchesParameters(GenericInstanceType genericEvent, TypeReference originalType, int count)
        {
            // matches event Action<T, T> eventName;
            var args = genericEvent.GenericArguments;
            for (var i = 0; i < count; i++)
            {
                if (args[i].FullName != originalType.FullName)
                    return false;
            }
            return true;
        }

        private static bool MatchesParameters(MethodDefinition method, TypeReference originalType, int count)
        {
            // matches void onValueChange(T oldValue, T newValue)
            var parameters = method.Parameters;
            for (var i = 0; i < count; i++)
            {
                if (parameters[i].ParameterType.FullName != originalType.FullName)
                    return false;
            }
            return true;
        }
    }
}
