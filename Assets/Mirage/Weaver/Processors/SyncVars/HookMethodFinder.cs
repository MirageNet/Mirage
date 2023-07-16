using System;
using System.Linq;
using Mirage.CodeGen;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal class SyncVarHook
    {
        public readonly MethodDefinition Method;
        public readonly EventDefinition Event;
        public readonly int ArgCount;


        public SyncVarHook(MethodDefinition method, int argCount)
        {
            Method = method;
            ArgCount = argCount;
        }
        public SyncVarHook(EventDefinition @event, int argCount)
        {
            Event = @event;
            ArgCount = argCount;
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
                case SyncHookType.MethodWith0Arg:
                case SyncHookType.MethodWith1Arg:
                case SyncHookType.MethodWith2Arg:
                    return FindMethod(syncVar, originalType, hookFunctionName, ArgCountFromType(hookType));
                case SyncHookType.EventWith0Arg:
                case SyncHookType.EventWith1Arg:
                case SyncHookType.EventWith2Arg:
                    return FindEvent(syncVar, originalType, hookFunctionName, ArgCountFromType(hookType));
            }
        }

        private static SyncVarHook FindAutomatic(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            SyncVarHook foundHook = null;

            for (var i = 0; i < 3; i++)
            {
                CheckHook(syncVar, hookFunctionName, ref foundHook, FindMethod(syncVar, originalType, hookFunctionName, i));
            }
            // we want to pass null for arg count here, because we are ok with any arg count
            CheckHook(syncVar, hookFunctionName, ref foundHook, FindEvent(syncVar, originalType, hookFunctionName, null));

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

        private static SyncVarHook FindMethod(FieldDefinition syncVar, TypeReference originalType, string hookFunctionName, int argCount)
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
                    return new SyncVarHook(method, argCount);
                }
            }

            // else throw saying args were wrong
            throw new HookMethodException($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'.", syncVar, methods.First());
        }

        private static SyncVarHook FindEvent(FieldDefinition syncVar, TypeReference originalType, string hookFunctionName, int? argCount)
        {
            // we can't have 2 events/fields with same name, so using `First` is ok here
            var @event = syncVar.DeclaringType.Events.FirstOrDefault(x => x.Name == hookFunctionName);
            if (@event == null)
                return null;

            var eventType = @event.EventType;
            if (!eventType.FullName.Contains("System.Action"))
            {
                ThrowWrongHookType(syncVar, @event, eventType, "Not System.Action");
            }

            // if it is not generic, then it has no args
            if (!eventType.IsGenericInstance)
            {
                // first check if we are expecting 0 args
                if (argCount.HasValue && argCount.Value != 0)
                    ThrowWrongHookType(syncVar, @event, eventType, "Generic mismatch");

                // the return 0 arg hook as ok
                return new SyncVarHook(@event, 0);
            }

            // this point on, we know it is generic
            var genericEvent = (GenericInstanceType)eventType;
            var args = genericEvent.GenericArguments;

            // check arg count
            if (argCount.HasValue)
            {
                if (args.Count != argCount)
                    ThrowWrongHookType(syncVar, @event, eventType, "Arg mismatch");
            }
            else
            {
                if (args.Count > 2)
                    ThrowWrongHookType(syncVar, @event, eventType, "Too many args");
            }

            // check param types
            if (!MatchesParameters(genericEvent, originalType, args.Count))
            {
                ThrowWrongHookType(syncVar, @event, eventType, "Param mismatch");
            }

            return new SyncVarHook(@event, args.Count);
        }

        private static void ThrowWrongHookType(FieldDefinition syncVar, EventDefinition @event, TypeReference eventType, string extra)
        {
            throw new HookMethodException($"Hook Event for '{syncVar.Name}' is invalid '{eventType.FullName}', Error Type: {extra}", @event);
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

        private static SyncHookType TypeFromArgCount(bool method, int argCount)
        {
            switch (argCount)
            {
                case 0:
                    return method ? SyncHookType.MethodWith0Arg : SyncHookType.EventWith0Arg;
                case 1:
                    return method ? SyncHookType.MethodWith1Arg : SyncHookType.EventWith1Arg;
                case 2:
                    return method ? SyncHookType.MethodWith2Arg : SyncHookType.EventWith2Arg;
                default:
                    throw new ArgumentOutOfRangeException(nameof(argCount), argCount, null);
            }
        }

        private static int ArgCountFromType(SyncHookType hookType)
        {
            switch (hookType)
            {
                case SyncHookType.MethodWith0Arg:
                case SyncHookType.EventWith0Arg:
                    return 0;
                case SyncHookType.MethodWith1Arg:
                case SyncHookType.EventWith1Arg:
                    return 1;
                case SyncHookType.MethodWith2Arg:
                case SyncHookType.EventWith2Arg:
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hookType), hookType, null);
            }
        }
    }
}
