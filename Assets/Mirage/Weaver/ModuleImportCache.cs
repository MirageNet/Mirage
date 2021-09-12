using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Mirage.Weaver
{
    /// <summary>
    /// Caches imports to avoid importing multiple times
    /// </summary>
    public sealed class ModuleImportCache
    {
        struct RefCounter<T>
        {
            public T reference;
            public int count;
        }
        public readonly ModuleDefinition Module;
        readonly Dictionary<object, RefCounter<MethodReference>> methodImports = new Dictionary<object, RefCounter<MethodReference>>();
        readonly Dictionary<object, RefCounter<TypeReference>> typeImports = new Dictionary<object, RefCounter<TypeReference>>();
        readonly Dictionary<object, RefCounter<FieldReference>> fieldImports = new Dictionary<object, RefCounter<FieldReference>>();
        TypeDefinition generatedClass;

        StreamWriter writer;
        public ModuleImportCache(ModuleDefinition module)
        {
            Module = module;

#if WEAVER_CACHE_DEBUG
            writer = new StreamWriter($"./Build/ImportCache/Cache_{module.Name}.log")
            {
                AutoFlush = true,
            };

            Console.WriteLine($"[WeaverDiagnostics] Cache logs enabled");
#else
            Console.WriteLine($"[WeaverDiagnostics] Cache logs disabled");
#endif 
        }
        ~ModuleImportCache()
        {
            writer?.Dispose();
            writer = null;
        }

        public void Close(long endTime)
        {
            if (writer != null)
            {
                WriteImportsToFile(endTime);
                writer.Dispose();
                writer = null;
            }
        }

        private void WriteImportsToFile(long endTime)
        {
            writer.WriteLine($"");
            writer.WriteLine($"--------");
            writer.WriteLine($"FINISHED");
            writer.WriteLine($"--------");

            {
                writer.WriteLine($"");
                writer.WriteLine($"Methods");
                writer.WriteLine($"--------");
                int saved = 0;
                int total = 0;
                foreach (RefCounter<MethodReference> found in methodImports.Values.OrderBy(x => x.reference.FullName).ThenBy(x => x.count))
                {
                    writer.WriteLine($"({found.count,3}) {found.reference.FullName}");
                    saved += found.count - 1;
                    total += found.count;
                }
                writer.WriteLine($"--------");
                writer.WriteLine($"Imported: {total} Saved by Caching:{saved}");
                writer.WriteLine($"--------");
            }

            {
                writer.WriteLine($"");
                writer.WriteLine($"Types");
                writer.WriteLine($"--------");
                int saved = 0;
                int total = 0;
                foreach (RefCounter<TypeReference> found in typeImports.Values.OrderBy(x => x.reference.FullName).ThenBy(x => x.count))
                {
                    writer.WriteLine($"({found.count,3}) {found.reference.FullName}");
                    saved += found.count - 1;
                    total += found.count;
                }
                writer.WriteLine($"--------");
                writer.WriteLine($"Imported: {total} Saved by Caching:{saved}");
                writer.WriteLine($"--------");
            }

            {
                writer.WriteLine($"");
                writer.WriteLine($"Fields");
                writer.WriteLine($"--------");
                int saved = 0;
                int total = 0;
                foreach (RefCounter<FieldReference> found in fieldImports.Values.OrderBy(x => x.reference.FullName).ThenBy(x => x.count))
                {
                    writer.WriteLine($"({found.count,3}) {found.reference.FullName}");
                    saved += found.count - 1;
                    total += found.count;
                }
                writer.WriteLine($"--------");
                writer.WriteLine($"Imported: {total} Saved by Caching:{saved}");
                writer.WriteLine($"--------");
            }

            writer.WriteLine($"");
            writer.WriteLine($"Weave Finished: {endTime}ms");
        }

        [System.Diagnostics.Conditional("WEAVER_CACHE_DEBUG")]
        void DebugLog(string text)
        {
            writer.WriteLine(text);
        }


        public TypeDefinition GeneratedClass()
        {
            // if file created, return it
            if (generatedClass != null)
                return generatedClass;

            // try find existing type
            generatedClass = Module.GetType("Mirage", "GeneratedNetworkCode");
            if (generatedClass != null)
                return generatedClass;

            // create new type
            TypeAttributes typeAttributes = TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed;
            generatedClass = new TypeDefinition("Mirage", "GeneratedNetworkCode", typeAttributes, ImportReference<object>());
            Module.Types.Add(generatedClass);
            return generatedClass;
        }

        #region Method Reference
        public MethodReference ImportReference(Expression<Action> expression) => ImportReference((LambdaExpression)expression);
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
        public MethodReference ImportReference<T>(Expression<Action<T>> expression) => ImportReference((LambdaExpression)expression);
        public MethodReference ImportReference<T1, T2>(Expression<Func<T1, T2>> expression) => ImportReference((LambdaExpression)expression);

        public MethodReference ImportReference(LambdaExpression expression)
        {
            if (expression.Body is MethodCallExpression outermostExpression)
            {
                MethodInfo methodInfo = outermostExpression.Method;

                return ImportReference(methodInfo);
            }

            if (expression.Body is NewExpression newExpression)
            {
                ConstructorInfo methodInfo = newExpression.Constructor;
                // constructor is null when creating default struct, eg `ArraySegment<T>()` or `Vector3()`
                methodInfo = methodInfo ?? newExpression.Type.GetConstructors()[0];

                return ImportReference(methodInfo);
            }

            if (expression.Body is MemberExpression memberExpression)
            {
                var property = memberExpression.Member as PropertyInfo;

                return ImportReference(property.GetMethod);
            }

            throw new ArgumentException($"Invalid Expression {expression.Body.GetType()}");
        }

        public MethodReference ImportReference(MethodReference methodReference)
        {
            if (!methodImports.TryGetValue(methodReference, out RefCounter<MethodReference> found))
            {
                found.reference = Module.ImportReference(methodReference);
            }
            found.count++;
            methodImports[methodReference] = found;

            DebugLog($"MR ({found.count,3}) {found.reference.FullName}");
            return found.reference;
        }
        public MethodReference ImportReference(MethodBase methodInfo)
        {
            if (!methodImports.TryGetValue(methodInfo, out RefCounter<MethodReference> found))
            {
                found.reference = Module.ImportReference(methodInfo);
            }
            found.count++;
            methodImports[methodInfo] = found;

            DebugLog($"MI ({found.count,3}) {found.reference.FullName}");
            return found.reference;
        }

        #endregion


        #region Type Reference

        public TypeReference ImportReference(TypeReference typeReference)
        {
            if (!typeImports.TryGetValue(typeReference, out RefCounter<TypeReference> found))
            {
                found.reference = Module.ImportReference(typeReference);
            }
            found.count++;
            typeImports[typeReference] = found;

            DebugLog($"TY ({found.count,3}) { found.reference.FullName}");
            return found.reference;
        }
        public TypeReference ImportReference<T>() => ImportReference(typeof(T));
        public TypeReference ImportReference(Type type)
        {
            if (!typeImports.TryGetValue(type, out RefCounter<TypeReference> found))
            {
                found.reference = Module.ImportReference(type);
            }
            found.count++;
            typeImports[type] = found;

            DebugLog($"TY ({found.count,3}) { found.reference.FullName}");
            return found.reference;
        }

        #endregion


        #region Field Reference

        public FieldReference ImportReference(FieldReference fieldReference)
        {
            if (!fieldImports.TryGetValue(fieldReference, out RefCounter<FieldReference> found))
            {
                found.reference = Module.ImportReference(fieldReference);
            }
            found.count++;
            fieldImports[fieldReference] = found;


            DebugLog($"FR ({found.count,3}) {found.reference.FullName}");
            return found.reference;
        }

        #endregion
    }
}
