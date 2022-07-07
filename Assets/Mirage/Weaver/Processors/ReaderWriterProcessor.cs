using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;

namespace Mirage.Weaver
{
    public class ReaderWriterProcessor
    {
        [Conditional("WEAVER_DEBUG_LOGS")]
        internal static void Log(string msg)
        {
            Console.Write($"[Weaver.ReaderWriterProcessor] {msg}\n");
        }

        private readonly HashSet<TypeReference> messages = new HashSet<TypeReference>(new TypeReferenceComparer());

        private readonly ModuleDefinition module;
        private readonly Readers readers;
        private readonly Writers writers;
        private readonly SerailizeExtensionHelper extensionHelper;

        /// <summary>
        /// Mirage's main module used to find built in extension methods and messages
        /// </summary>
        static Module MirageModule => typeof(NetworkWriter).Module;

        public ReaderWriterProcessor(ModuleDefinition module, Readers readers, Writers writers)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
            extensionHelper = new SerailizeExtensionHelper(module, readers, writers);
        }

        public bool Process()
        {
            messages.Clear();

            bool processed = FindAllExtensionMethods();

            LoadBuiltinMessages();

            // store how many writers are found, we need to check if currentModule adds any
            int writeCount = writers.Count;
            int readCount = readers.Count;
            ProcessModule();

            // we need to check if any methods are created from FindAllExtensionMethods or ProcessModule
            return processed || writers.Count != writeCount || readers.Count != readCount;
        }

        /// <summary>
        /// Gets all extension methods in current assembly and all references
        /// </summary>
        private bool FindAllExtensionMethods()
        {
            var assemblies = new List<AssemblyDefinition>();
            // load all references
            foreach (AssemblyNameReference reference in module.AssemblyReferences)
            {
                AssemblyDefinition assembly = module.AssemblyResolver.Resolve(reference);
                assemblies.Add(assembly);
            }

            // store how many writers are found, we need to check if currentModule adds any
            int writeCount = writers.Count;
            int readCount = readers.Count;
            FindExtensionMethodsInAssembly(module.Assembly);
            // have any been added?
            bool processed = writers.Count != writeCount || readers.Count != readCount;

            // process all references
            foreach (AssemblyDefinition assembly in assemblies)
            {
                FindExtensionMethodsInAssembly(assembly);
            }

            return processed;
        }

        private void FindExtensionMethodsInAssembly(AssemblyDefinition assembly)
        {
            Log($"Looking for extension methods in {assembly.FullName}");
            foreach (ModuleDefinition module in assembly.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    TypeDefinition resolved = type.Resolve();
                    extensionHelper.RegisterExtensionMethodsInType(resolved);
                }
            }
        }

        /// <summary>
        /// Find NetworkMessage in Mirage.dll and ensure that they have serailize functions
        /// </summary>
        private void LoadBuiltinMessages()
        {
            IEnumerable<Type> types = MirageModule.GetTypes().Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null);
            foreach (Type type in types)
            {
                TypeReference typeReference = module.ImportReference(type);
                writers.TryGetFunction(typeReference, null);
                readers.TryGetFunction(typeReference, null);
                messages.Add(typeReference);
            }
        }

        void ProcessModule()
        {
            // create copy incase we modify types
            var types = new List<TypeDefinition>(module.Types);

            // find NetworkMessages
            foreach (TypeDefinition klass in types)
                CheckForNetworkMessage(klass);

            // Generate readers and writers
            // find all the Send<> and Register<> calls and generate
            // readers and writers for them.
            CodePass.ForEachInstruction(module, GenerateReadersWriters);
        }

        private void CheckForNetworkMessage(TypeDefinition klass)
        {
            if (klass.HasCustomAttribute<NetworkMessageAttribute>())
            {
                readers.TryGetFunction(klass, null);
                writers.TryGetFunction(klass, null);
                messages.Add(klass);
            }

            foreach (TypeDefinition nestedClass in klass.NestedTypes)
            {
                CheckForNetworkMessage(nestedClass);
            }
        }

        private Instruction GenerateReadersWriters(MethodDefinition _, Instruction instruction, SequencePoint sequencePoint)
        {
            if (instruction.OpCode == OpCodes.Ldsfld)
            {
                GenerateReadersWriters((FieldReference)instruction.Operand, sequencePoint);
            }

            // We are looking for calls to some specific types
            if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
            {
                GenerateReadersWriters((MethodReference)instruction.Operand, sequencePoint);
            }

            return instruction;
        }

        private void GenerateReadersWriters(FieldReference field, SequencePoint sequencePoint)
        {
            TypeReference type = field.DeclaringType;

            if (type.Is(typeof(Writer<>)) || type.Is(typeof(Reader<>)) && type.IsGenericInstance)
            {
                var typeGenericInstance = (GenericInstanceType)type;

                TypeReference parameterType = typeGenericInstance.GenericArguments[0];

                GenerateReadersWriters(parameterType, sequencePoint);
            }
        }

        private void GenerateReadersWriters(MethodReference method, SequencePoint sequencePoint)
        {
            if (!method.IsGenericInstance)
                return;

            // generate methods for message or types used by generic read/write
            bool isMessage = IsMessageMethod(method);

            bool generate = isMessage ||
                IsReadWriteMethod(method);

            if (generate)
            {
                var instanceMethod = (GenericInstanceMethod)method;
                TypeReference parameterType = instanceMethod.GenericArguments[0];

                if (parameterType.IsGenericParameter)
                    return;

                GenerateReadersWriters(parameterType, sequencePoint);
                if (isMessage)
                    messages.Add(parameterType);
            }
        }

        private void GenerateReadersWriters(TypeReference parameterType, SequencePoint sequencePoint)
        {
            if (!parameterType.IsGenericParameter && parameterType.CanBeResolved())
            {
                TypeDefinition typeDefinition = parameterType.Resolve();

                if (typeDefinition.IsClass && !typeDefinition.IsValueType)
                {
                    MethodDefinition constructor = typeDefinition.GetMethod(".ctor");

                    bool hasAccess = constructor.IsPublic
                        || constructor.IsAssembly && typeDefinition.Module == module;

                    if (!hasAccess)
                        return;
                }

                writers.TryGetFunction(parameterType, sequencePoint);
                readers.TryGetFunction(parameterType, sequencePoint);
            }
        }

        /// <summary>
        /// is method used to send a message? if it use then T is a message and needs read/write functions
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static bool IsMessageMethod(MethodReference method)
        {
            return
                method.Is(typeof(MessagePacker), nameof(MessagePacker.Pack)) ||
                method.Is(typeof(MessagePacker), nameof(MessagePacker.GetId)) ||
                method.Is(typeof(MessagePacker), nameof(MessagePacker.Unpack)) ||
                method.Is<IMessageSender>(nameof(IMessageSender.Send)) ||
                method.Is<IMessageReceiver>(nameof(IMessageReceiver.RegisterHandler)) ||
                method.Is<IMessageReceiver>(nameof(IMessageReceiver.UnregisterHandler)) ||
                method.Is<NetworkPlayer>(nameof(NetworkPlayer.Send)) ||
                method.Is<MessageHandler>(nameof(MessageHandler.RegisterHandler)) ||
                method.Is<MessageHandler>(nameof(MessageHandler.UnregisterHandler)) ||
                method.Is<NetworkClient>(nameof(NetworkClient.Send)) ||
                method.Is<NetworkServer>(nameof(NetworkServer.SendToAll)) ||
                method.Is<NetworkServer>(nameof(NetworkServer.SendToMany)) ||
                method.Is<INetworkServer>(nameof(INetworkServer.SendToAll));
        }

        private static bool IsReadWriteMethod(MethodReference method)
        {
            return
                method.Is(typeof(GenericTypesSerializationExtensions), nameof(GenericTypesSerializationExtensions.Write)) ||
                method.Is(typeof(GenericTypesSerializationExtensions), nameof(GenericTypesSerializationExtensions.Read));
        }



        private static bool IsEditorAssembly(ModuleDefinition module)
        {
            return module.AssemblyReferences.Any(assemblyReference =>
                assemblyReference.Name == "Mirage.Editor"
                );
        }

        /// <summary>
        /// Creates a method that will store all the readers and writers into
        /// <see cref="Writer{T}.Write"/> and <see cref="Reader{T}.Read"/>
        ///
        /// The method will be marked InitializeOnLoadMethodAttribute so it gets
        /// executed before mirror runtime code
        /// </summary>
        /// <param name="currentAssembly"></param>
        public void InitializeReaderAndWriters()
        {
            MethodDefinition rwInitializer = module.GeneratedClass().AddMethod(
                "InitReadWriters",
                Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static);

            ConstructorInfo attributeconstructor = typeof(RuntimeInitializeOnLoadMethodAttribute).GetConstructor(new[] { typeof(RuntimeInitializeLoadType) });

            var customAttributeRef = new CustomAttribute(module.ImportReference(attributeconstructor));
            customAttributeRef.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference<RuntimeInitializeLoadType>(), RuntimeInitializeLoadType.BeforeSceneLoad));
            rwInitializer.CustomAttributes.Add(customAttributeRef);

            if (IsEditorAssembly(module))
            {
                // editor assembly,  add InitializeOnLoadMethod too.  Useful for the editor tests
                ConstructorInfo initializeOnLoadConstructor = typeof(InitializeOnLoadMethodAttribute).GetConstructor(new Type[0]);
                var initializeCustomConstructorRef = new CustomAttribute(module.ImportReference(initializeOnLoadConstructor));
                rwInitializer.CustomAttributes.Add(initializeCustomConstructorRef);
            }

            ILProcessor worker = rwInitializer.Body.GetILProcessor();

            writers.InitializeWriters(worker);
            readers.InitializeReaders(worker);

            RegisterMessages(worker);

            worker.Append(worker.Create(OpCodes.Ret));
        }

        private void RegisterMessages(ILProcessor worker)
        {
            MethodInfo method = typeof(MessagePacker).GetMethod(nameof(MessagePacker.RegisterMessage));
            MethodReference registerMethod = module.ImportReference(method);

            foreach (TypeReference message in messages)
            {
                var genericMethodCall = new GenericInstanceMethod(registerMethod);
                genericMethodCall.GenericArguments.Add(module.ImportReference(message));
                worker.Append(worker.Create(OpCodes.Call, genericMethodCall));
            }
        }
    }

    /// <summary>
    /// Helps get Extension methods using either reflection or cecil
    /// </summary>
    public class SerailizeExtensionHelper
    {
        private readonly ModuleDefinition module;
        private readonly Readers readers;
        private readonly Writers writers;

        public SerailizeExtensionHelper(ModuleDefinition module, Readers readers, Writers writers)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
        }


        public void RegisterExtensionMethodsInType(Type type)
        {
            // only check static types
            if (!IsStatic(type))
                return;

            IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                   .Where(IsExtension)
                   .Where(NotGeneric)
                   .Where(NotIgnored);

            foreach (MethodInfo method in methods)
            {
                if (IsWriterMethod(method))
                {
                    RegisterWriter(method);
                }

                if (IsReaderMethod(method))
                {
                    RegisterReader(method);
                }
            }
        }
        public void RegisterExtensionMethodsInType(TypeDefinition type)
        {
            // only check static types
            if (!IsStatic(type))
                return;

            IEnumerable<MethodDefinition> methods = type.Methods
                   .Where(IsExtension)
                   .Where(NotGeneric)
                   .Where(NotIgnored);

            foreach (MethodDefinition method in methods)
            {
                if (IsWriterMethod(method))
                {
                    RegisterWriter(method);
                }

                if (IsReaderMethod(method))
                {
                    RegisterReader(method);
                }
            }
        }

        /// <summary>
        /// static classes are declared abstract and sealed at the IL level.
        /// <see href="https://stackoverflow.com/a/1175901/8479976"/>
        /// </summary>
        private static bool IsStatic(Type t) => t.IsSealed && t.IsAbstract;
        private static bool IsStatic(TypeDefinition t) => t.IsSealed && t.IsAbstract;

        private static bool IsExtension(MethodInfo method) => Attribute.IsDefined(method, typeof(ExtensionAttribute));
        private static bool IsExtension(MethodDefinition method) => method.HasCustomAttribute<ExtensionAttribute>();
        private static bool NotGeneric(MethodInfo method) => !method.IsGenericMethod;
        private static bool NotGeneric(MethodDefinition method) => !method.IsGenericInstance && !method.HasGenericParameters;

        /// <returns>true if method does not have <see cref="WeaverIgnoreAttribute"/></returns>
        private static bool NotIgnored(MethodInfo method) => !Attribute.IsDefined(method, typeof(WeaverIgnoreAttribute));
        /// <returns>true if method does not have <see cref="WeaverIgnoreAttribute"/></returns>
        private static bool NotIgnored(MethodDefinition method) => !method.HasCustomAttribute<WeaverIgnoreAttribute>();


        private static bool IsWriterMethod(MethodInfo method)
        {
            if (method.GetParameters().Length != 2)
                return false;

            if (method.GetParameters()[0].ParameterType.FullName != typeof(NetworkWriter).FullName)
                return false;

            if (method.ReturnType != typeof(void))
                return false;

            return true;
        }
        private bool IsWriterMethod(MethodDefinition method)
        {
            if (method.Parameters.Count != 2)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(NetworkWriter).FullName)
                return false;

            if (!method.ReturnType.Is(typeof(void)))
                return false;

            return true;
        }

        private static bool IsReaderMethod(MethodInfo method)
        {
            if (method.GetParameters().Length != 1)
                return false;

            if (method.GetParameters()[0].ParameterType.FullName != typeof(NetworkReader).FullName)
                return false;

            if (method.ReturnType == typeof(void))
                return false;

            return true;
        }
        private bool IsReaderMethod(MethodDefinition method)
        {
            if (method.Parameters.Count != 1)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(NetworkReader).FullName)
                return false;

            if (method.ReturnType.Is(typeof(void)))
                return false;

            return true;
        }

        private void RegisterWriter(MethodInfo method)
        {
            ReaderWriterProcessor.Log($"Found writer extension methods: {method.Name}");

            Type dataType = method.GetParameters()[1].ParameterType;
            writers.Register(module.ImportReference(dataType), module.ImportReference(method));
        }
        private void RegisterWriter(MethodDefinition method)
        {
            ReaderWriterProcessor.Log($"Found writer extension methods: {method.Name}");

            TypeReference dataType = method.Parameters[1].ParameterType;
            writers.Register(module.ImportReference(dataType), module.ImportReference(method));
        }


        private void RegisterReader(MethodInfo method)
        {
            ReaderWriterProcessor.Log($"Found reader extension methods: {method.Name}");
            readers.Register(module.ImportReference(method.ReturnType), module.ImportReference(method));
        }
        private void RegisterReader(MethodDefinition method)
        {
            ReaderWriterProcessor.Log($"Found reader extension methods: {method.Name}");
            readers.Register(module.ImportReference(method.ReturnType), module.ImportReference(method));
        }
    }
}
