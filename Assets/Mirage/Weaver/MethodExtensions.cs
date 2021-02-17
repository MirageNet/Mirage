using System;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Convenient extensions for modifying methods
    /// </summary>
    public static class MethodExtensions
    {
        public static ParameterDefinition AddParam<T>(this MethodDefinition method, string name, ParameterAttributes attributes = ParameterAttributes.None)
            => AddParam(method, method.Module.ImportReference(typeof(T)), name, attributes);

        public static ParameterDefinition AddParam(this MethodDefinition method, TypeReference typeRef, string name, ParameterAttributes attributes = ParameterAttributes.None)
        {
            var param = new ParameterDefinition(name, attributes, typeRef);
            method.Parameters.Add(param);
            return param;
        }

        public static VariableDefinition AddLocal<T>(this MethodDefinition method) => AddLocal(method, method.Module.ImportReference(typeof(T)));

        public static VariableDefinition AddLocal(this MethodDefinition method, TypeReference type)
        {
            var local = new VariableDefinition(type);
            method.Body.Variables.Add(local);
            return local;
        }

        public static Instruction Create(this ILProcessor worker, OpCode code, Expression<Action> expression)
        {
            MethodReference typeref = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeref);
        }

        public static Instruction Create<T>(this ILProcessor worker, OpCode code, Expression<Action<T>> expression)
        {
            MethodReference typeref = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeref);
        }

        public static Instruction Create<T, TR>(this ILProcessor worker, OpCode code, Expression<Func<T, TR>> expression)
        {
            MethodReference typeref = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeref);
        }

        public static SequencePoint GetSequencePoint(this MethodDefinition method, Instruction instruction)
        {
            SequencePoint sequencePoint = method.DebugInformation.GetSequencePoint(instruction);
            return sequencePoint;
        }

        /// <summary>
        /// Duplicates a method reference. 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="declaringType">A new declaring type. Set to null to be the same as the base method.</param>
        /// <returns></returns>
        public static MethodReference Duplicate(this MethodReference method, TypeReference declaringType = null)
        {
            MethodReference newMethod = new MethodReference(method.Name, method.ReturnType, declaringType ?? method.DeclaringType)
            {
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis
            };

            if (method.HasParameters)
            {
                // Add back all the parameters.
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    newMethod.Parameters.Add(new ParameterDefinition(method.Parameters[i].Name,
                                                                     method.Parameters[i].Attributes,
                                                                     method.Parameters[i].ParameterType));
                }
            }

            return newMethod;
        }
    }
}
