using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Serialization
{
    /// <summary>
    /// a class that holds writers for the different types
    /// Note that c# creates a different static variable for each
    /// type
    /// This will be populated by the weaver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Writer<T>
    {
        /// <summary>
        /// Priority of current write function <see cref="SerializeExtensionAttribute"/>
        /// </summary>
        private static int currentPriority;

        public static Action<NetworkWriter, T> Write { get; private set; }

        public static void SetWriter(Action<NetworkWriter, T> method, int newPriority)
        {
            // if first writer or higher priority
            if (Write == null || newPriority > currentPriority)
            {
                Write = method;
                currentPriority = newPriority;
            }
            else if (newPriority == currentPriority
                // todo remove this hack
                // check names because current weaver outputs SetWriter to multiple assemblies for same method
                && fullName(method.Method) != fullName(Write.Method))
            {
                if (GenericTypesSerializationExtensions.Logger.WarnEnabled())
                    GenericTypesSerializationExtensions.Logger.LogWarning($"Can now add new writer because it has same priority as current one: " +
                        $"priority={newPriority}, new={fullName(method.Method)}, old={fullName(Write.Method)}");
            }

        }
        static string fullName(MethodInfo info)
        {
            return $"{info.DeclaringType.FullName}::{info.Name}";
        }
    }

    /// <summary>
    /// a class that holds readers for the different types
    /// Note that c# creates a different static variable for each
    /// type
    /// This will be populated by the weaver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Reader<T>
    {
        /// <summary>
        /// Priority of current write function <see cref="SerializeExtensionAttribute"/>
        /// </summary>
        private static int currentPriority;

        public static Func<NetworkReader, T> Read { get; set; }

        public static void SetReader(Func<NetworkReader, T> method, int newPriority)
        {
            // if first writer or higher priority
            if (Read == null || newPriority > currentPriority)
            {
                Read = method;
                currentPriority = newPriority;
            }
            else if (newPriority == currentPriority)
            {
                if (GenericTypesSerializationExtensions.Logger.WarnEnabled())
                    GenericTypesSerializationExtensions.Logger.LogWarning($"Can now add new read because it has same priority as current one: priority={newPriority}, new={method.Method.Name}, old={Read.Method.Name}");
            }
        }
    }

    public static class GenericTypesSerializationExtensions
    {
        // make this public so it can be shared by the static generic classes Write<T> and Reader<T>
        public static readonly ILogger Logger = LogFactory.GetLogger("Mirage.Serialization");

        /// <summary>
        /// Writes any type that mirror supports
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // do not include these as serialize function because they are used directly by weaver
        [WeaverIgnore]
        public static void Write<T>(this NetworkWriter writer, T value)
        {
            if (Writer<T>.Write == null)
                throw new KeyNotFoundException($"No writer found for {typeof(T)}. See https://miragenet.github.io/Mirage/Articles/General/Troubleshooting.html for details");

            Writer<T>.Write(writer, value);
        }

        /// <summary>
        /// Reads any data type that mirror supports
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // do not include these as serialize function because they are used directly by weaver
        [WeaverIgnore]
        public static T Read<T>(this NetworkReader reader)
        {
            if (Reader<T>.Read == null)
                throw new KeyNotFoundException($"No reader found for {typeof(T)}. See https://miragenet.github.io/Mirage/Articles/General/Troubleshooting.html for details");

            return Reader<T>.Read(reader);
        }
    }
}
