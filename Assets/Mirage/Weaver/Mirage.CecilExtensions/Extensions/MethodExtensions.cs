using System;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace Mirage.CodeGen
{
    /// <summary>
    /// Convenient extensions for modifying methods
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Adds a method parameter of type <typeparamref name="T"/> to the <paramref name="method"/>
        /// </summary>
        public static ParameterDefinition AddParam<T>(this MethodDefinition method, string name, ParameterAttributes attributes = ParameterAttributes.None)
            => AddParam(method, method.Module.ImportReference(typeof(T)), name, attributes);

        /// <summary>
        /// Adds a method parameter of type <paramref name="type"/> to the <paramref name="method"/>
        /// </summary>
        public static ParameterDefinition AddParam(this MethodDefinition method, Type type, string name, ParameterAttributes attributes = ParameterAttributes.None)
        => AddParam(method, method.Module.ImportReference(type), name, attributes);

        /// <summary>
        /// Adds a method parameter of type <paramref name="type"/> to the <paramref name="method"/>
        /// </summary>
        public static ParameterDefinition AddParam(this MethodDefinition method, TypeReference typeRef, string name, ParameterAttributes attributes = ParameterAttributes.None)
        {
            var param = new ParameterDefinition(name, attributes, typeRef);
            method.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// Adds a local variable of type <typeparamref name="T"/> to the <paramref name="method"/>
        /// </summary>
        public static VariableDefinition AddLocal<T>(this MethodDefinition method) => AddLocal(method, method.Module.ImportReference(typeof(T)));

        /// <summary>
        /// Adds a local variable of type <paramref name="type"/> to the <paramref name="method"/>
        /// </summary>
        public static VariableDefinition AddLocal(this MethodDefinition method, TypeReference type)
        {
            var local = new VariableDefinition(type);
            method.Body.Variables.Add(local);
            return local;
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static Instruction Create(this ILProcessor worker, OpCode code, LambdaExpression expression)
        {
            var typeRef = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static Instruction Create(this ILProcessor worker, OpCode code, Expression<Action> expression)
        {
            var typeRef = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static Instruction Create<T>(this ILProcessor worker, OpCode code, Expression<Action<T>> expression)
        {
            var typeRef = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static Instruction Create<T, TR>(this ILProcessor worker, OpCode code, Expression<Func<T, TR>> expression)
        {
            var typeRef = worker.Body.Method.Module.ImportReference(expression);
            return worker.Create(code, typeRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static void Emit(this ILProcessor worker, OpCode code, LambdaExpression expression)
        {
            var methodRef = worker.Body.Method.Module.ImportReference(expression);
            worker.Emit(code, methodRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static void Emit(this ILProcessor worker, OpCode code, Expression<Action> expression)
        {
            var methodRef = worker.Body.Method.Module.ImportReference(expression);
            worker.Emit(code, methodRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static void Emit<T>(this ILProcessor worker, OpCode code, Expression<Action<T>> expression)
        {
            var methodRef = worker.Body.Method.Module.ImportReference(expression);
            worker.Emit(code, methodRef);
        }

        /// <summary>
        /// Imports the <paramref name="expression"/> and creates an instruction using the method reference
        /// </summary>
        public static void Emit<T, TR>(this ILProcessor worker, OpCode code, Expression<Func<T, TR>> expression)
        {
            var methodRef = worker.Body.Method.Module.ImportReference(expression);
            worker.Emit(code, methodRef);
        }
    }
}
