using System;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Mirage.CodeGen
{
    public static class ModuleExtension
    {
        /// <summary>
        /// Imports an expression
        /// </summary>
        public static MethodReference ImportReference(this ModuleDefinition module, Expression<Action> expression) => ImportReference(module, (LambdaExpression)expression);

        /// <summary>
        /// this can be used to import reference to a non-static method
        /// <para>
        /// for example, <code>(NetworkWriter writer) => writer.Write(default, default)</code>
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodReference ImportReference<T>(this ModuleDefinition module, Expression<Action<T>> expression) => ImportReference(module, (LambdaExpression)expression);
        public static MethodReference ImportReference<T1, T2>(this ModuleDefinition module, Expression<Func<T1, T2>> expression) => ImportReference(module, (LambdaExpression)expression);

        public static TypeReference ImportReference<T>(this ModuleDefinition module) => module.ImportReference(typeof(T));

        public static MethodReference ImportReference(this ModuleDefinition module, LambdaExpression expression)
        {
            if (expression.Body is MethodCallExpression outermostExpression)
            {
                var methodInfo = outermostExpression.Method;
                return module.ImportReference(methodInfo);
            }

            if (expression.Body is NewExpression newExpression)
            {
                var methodInfo = newExpression.Constructor;
                // constructor is null when creating an ArraySegment<object>
                methodInfo = methodInfo ?? newExpression.Type.GetConstructors()[0];
                return module.ImportReference(methodInfo);
            }

            if (expression.Body is MemberExpression memberExpression)
            {
                var property = memberExpression.Member as PropertyInfo;
                return module.ImportReference(property.GetMethod);
            }

            throw new ArgumentException($"Invalid Expression {expression.Body.GetType()}");
        }


        public static TypeDefinition GeneratedClass(this ModuleDefinition module)
        {
            var type = module.GetType("Mirage", "GeneratedNetworkCode");

            if (type != null)
                return type;

            type = new TypeDefinition("Mirage", "GeneratedNetworkCode",
                        TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed,
                        module.ImportReference<object>());
            module.Types.Add(type);
            return type;
        }
    }
}
