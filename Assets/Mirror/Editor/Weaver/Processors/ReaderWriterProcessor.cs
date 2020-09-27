// finds all readers and writers and register them
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor.Compilation;
using UnityEditor;

namespace Mirror.Weaver
{
    public static class ReaderWriterProcessor
    {
        public static void Process(AssemblyDefinition CurrentAssembly)
        {
            Readers.Init();
            Writers.Init();

            foreach (Assembly unityAsm in CompilationPipeline.GetAssemblies())
            {
                if (unityAsm.name != CurrentAssembly.Name.Name)
                {
                    try
                    {
                        using (var asmResolver = new DefaultAssemblyResolver())
                        using (var assembly = AssemblyDefinition.ReadAssembly(unityAsm.outputPath, new ReaderParameters { ReadWrite = false, ReadSymbols = false, AssemblyResolver = asmResolver }))
                        {
                            ProcessAssemblyClasses(CurrentAssembly, assembly);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        // During first import,  this gets called before some assemblies
                        // are built,  just skip them
                    }
                }
            }

            ProcessAssemblyClasses(CurrentAssembly, CurrentAssembly);
        }

        static void ProcessAssemblyClasses(AssemblyDefinition CurrentAssembly, AssemblyDefinition assembly)
        {
            foreach (TypeDefinition klass in assembly.MainModule.Types)
            {
                // extension methods only live in static classes
                // static classes are represented as sealed and abstract
                if (klass.IsAbstract && klass.IsSealed)
                {
                    LoadDeclaredWriters(CurrentAssembly, klass);
                    LoadDeclaredReaders(CurrentAssembly, klass);
                }
            }

            foreach (TypeDefinition klass in assembly.MainModule.Types)
            {             
                // Generate readers and writers
                // find all the Send<> and Register<> calls and generate
                // readers and writers for them.
                GenerateReadersWriters(CurrentAssembly, klass);
            }
        }

        private static void GenerateReadersWriters(AssemblyDefinition currentAssembly, TypeDefinition klass)
        {
            foreach (MethodDefinition method in klass.Methods)
            {
                GenerateReadersWriters(currentAssembly, method);
            }
        }

        private static void GenerateReadersWriters(AssemblyDefinition currentAssembly, MethodDefinition method)
        {
            if (method.IsAbstract || method.Body == null)
                return;

            foreach (Instruction instruction in method.Body.Instructions)
            {
                GenerateReadersWriters(currentAssembly, instruction);
            }
        }


        private static void GenerateReadersWriters(AssemblyDefinition currentAssembly, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldsfld)
            {
                GenerateReadersWriters(currentAssembly, (FieldReference)instruction.Operand);
            }

            // We are looking for calls to some specific types
            if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
            {
                GenerateReadersWriters(currentAssembly, (MethodReference)instruction.Operand);
            }
        }

        private static void GenerateReadersWriters(AssemblyDefinition currentAssembly, FieldReference field)
        {
            TypeReference type = field.DeclaringType;

            if (type.Is(typeof(Writer<>)) || type.Is(typeof(Reader<>)) && type.IsGenericInstance)
            {
                var typeGenericInstance = (GenericInstanceType)type;

                TypeReference parameterType = typeGenericInstance.GenericArguments[0];

                if (!parameterType.IsGenericParameter)
                {
                    parameterType = currentAssembly.MainModule.ImportReference(parameterType);
                    Writers.GetWriteFunc(parameterType);
                    Readers.GetReadFunc(parameterType);
                }
            }
        }

        private static void GenerateReadersWriters(AssemblyDefinition currentAssembly, MethodReference method)
        {
            if (!method.IsGenericInstance)
                return;

            TypeReference declaringType = method.DeclaringType;

            bool generate = false;
            generate |= declaringType.Is(typeof(MessagePacker)) && method.Name == nameof(MessagePacker.Pack);
            generate |= declaringType.Is(typeof(MessagePacker)) && method.Name == nameof(MessagePacker.GetId);
            generate |= declaringType.Is(typeof(MessagePacker)) && method.Name == nameof(MessagePacker.Unpack);
            generate |= declaringType.Is(typeof(NetworkWriter)) && method.Name == nameof(NetworkWriter.WriteMessage);
            generate |= declaringType.Is(typeof(NetworkReader)) && method.Name == nameof(NetworkReader.ReadMessage);
            generate |= declaringType.Is(typeof(IMessageHandler)) && method.Name == nameof(IMessageHandler.Send);
            generate |= declaringType.Is(typeof(IMessageHandler)) && method.Name == nameof(IMessageHandler.SendAsync);
            generate |= declaringType.Is(typeof(IMessageHandler)) && method.Name == nameof(IMessageHandler.RegisterHandler);
            generate |= declaringType.Is(typeof(IMessageHandler)) && method.Name == nameof(IMessageHandler.UnregisterHandler);
            generate |= declaringType.Is(typeof(NetworkClient)) && method.Name == nameof(NetworkClient.Send);
            generate |= declaringType.Is(typeof(NetworkClient)) && method.Name == nameof(NetworkClient.SendAsync);
            generate |= declaringType.Is(typeof(NetworkServer)) && method.Name == nameof(NetworkServer.SendToAll);
            generate |= declaringType.Is(typeof(NetworkServer)) && method.Name == nameof(NetworkServer.SendToClientOfPlayer);
            generate |= declaringType.Is(typeof(NetworkServer)) && method.Name == nameof(NetworkServer.SendToReady);

            if (generate)
            {
                var instanceMethod = (GenericInstanceMethod)method;
                TypeReference parameter = instanceMethod.GenericArguments[0];
                if (!parameter.IsGenericParameter)
                {
                    parameter = currentAssembly.MainModule.ImportReference(parameter);
                    Readers.GetReadFunc(parameter);
                    Writers.GetWriteFunc(parameter);
                }
            }
        }

        static void LoadDeclaredWriters(AssemblyDefinition currentAssembly, TypeDefinition klass)
        {
            // register all the writers in this class.  Skip the ones with wrong signature
            foreach (MethodDefinition method in klass.Methods)
            {
                if (method.Parameters.Count != 2)
                    continue;

                if (!method.Parameters[0].ParameterType.Is<NetworkWriter>())
                    continue;

                if (!method.ReturnType.Is(typeof(void)))
                    continue;

                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>())
                    continue;

                if (method.HasGenericParameters)
                    continue;

                TypeReference dataType = method.Parameters[1].ParameterType;
                Writers.Register(dataType, currentAssembly.MainModule.ImportReference(method));
            }
        }

        static void LoadDeclaredReaders(AssemblyDefinition currentAssembly, TypeDefinition klass)
        {
            // register all the reader in this class.  Skip the ones with wrong signature
            foreach (MethodDefinition method in klass.Methods)
            {
                if (method.Parameters.Count != 1)
                    continue;

                if (!method.Parameters[0].ParameterType.Is<NetworkReader>())
                    continue;

                if (method.ReturnType.Is(typeof(void)))
                    continue;

                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>())
                    continue;

                if (method.HasGenericParameters)
                    continue;

                Readers.Register(method.ReturnType, currentAssembly.MainModule.ImportReference(method));
            }
        }

        public static void GenerateRWRegister(AssemblyDefinition currentAssembly)
        {
            var rwInitializer = new MethodDefinition("InitReadWriters", MethodAttributes.Public |
                    MethodAttributes.Static,
                    WeaverTypes.Import(typeof(void)));

            System.Reflection.ConstructorInfo attributeconstructor = typeof(InitializeOnLoadMethodAttribute).GetConstructors()[0];

            var customAttributeRef = new CustomAttribute(currentAssembly.MainModule.ImportReference(attributeconstructor));
            rwInitializer.CustomAttributes.Add(customAttributeRef);

            ILProcessor worker = rwInitializer.Body.GetILProcessor();

            Writers.GenerateRegister(worker);
            Readers.GenerateRegister(worker);

            worker.Append(worker.Create(OpCodes.Ret));

            Weaver.ConfirmGeneratedCodeClass();
            TypeDefinition generateClass = Weaver.WeaveLists.generateContainerClass;

            generateClass.Methods.Add(rwInitializer);
        }
    }
}
