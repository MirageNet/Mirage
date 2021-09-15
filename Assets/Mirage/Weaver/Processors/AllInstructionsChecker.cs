using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Mirage.Weaver
{
    // todo move to codepass
    public class AllInstructionsChecker
    {
        private readonly IWeaverLogger logger;
        private readonly ModuleImportCache moduleCache;
        private readonly Readers readers;
        private readonly Writers writers;
        private readonly ReaderWriterProcessor readerWriterProcessor;
        private readonly PropertySiteProcessor propertySiteProcessor;

        public AllInstructionsChecker(IWeaverLogger logger, ModuleImportCache moduleCache, Readers readers, Writers writers, ReaderWriterProcessor readerWriterProcessor, PropertySiteProcessor propertySiteProcessor)
        {
            this.logger = logger;
            this.moduleCache = moduleCache;
            this.readers = readers;
            this.writers = writers;
            this.readerWriterProcessor = readerWriterProcessor;
            this.propertySiteProcessor = propertySiteProcessor;
        }

        /// <summary>
        /// Checks both <see cref="ReaderWriterProcessor"/> and <see cref="PropertySiteProcessor"/>
        /// <para>
        ///     Checks all Instruction and finds any Send/Register/Write calls to the generic methods. If any are found it will generate write/read functions for the type used
        /// </para>
        /// <para>
        ///     Checks all Instruction and finds any syncvar fields and replaces them with the properties
        /// </para>
        /// </summary>
        public bool CheckAllInstructions()
        {
            int writeCount = writers.Count;
            int readCount = readers.Count;

            // Generate readers and writers
            // find all the Send<> and Register<> calls and generate
            // readers and writers for them.

            // old code:
            // CodePass.ForEachInstruction(moduleCache.Module, (md, instr, sequencePoint) => GenerateReadersWriters(instr, sequencePoint));
            List<MethodDefinition> methods = CodePass.GetAllMethods(moduleCache.Module);
            for (int i = 0; i < methods.Count; i++)
            {
                MethodDefinition m = methods[i];
                MethodBody body = GetValidBody(m);
                if (body != null)
                {
                    ProcessMethod(m, body, WeavedMethods(m));
                }
            }

            // did we create any new functions?
            return writers.Count != writeCount || readers.Count != readCount;
        }

        private static bool WeavedMethods(MethodDefinition md)
        {
            return
                md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName ||
                md.Name.StartsWith(RpcProcessor.InvokeRpcPrefix) ||
                md.IsConstructor;
        }

        static MethodBody GetValidBody(MethodDefinition m)
        {
            if (m.IsAbstract) { return null; }
            MethodBody body = m.Body;
            if (body == null) { return null; }
            if (body.Instructions == null) { return null; }
            if (body.CodeSize <= 0) { return null; }

            return body;
        }

        /// <summary>
        /// </summary>
        /// <param name="md"></param>
        /// <param name="body"></param>
        /// <param name="weavedMethod">Weaved methods are ignored by PropertySiteProcessor</param>
        void ProcessMethod(MethodDefinition md, MethodBody body, bool weavedMethod)
        {
            Instruction instr = body.Instructions[0];
            while (instr != null)
            {
                try
                {
                    readerWriterProcessor.GenerateReadersWriters(instr);
                }
                catch (SerializeFunctionException e)
                {
                    SequencePoint sq = CodePass.GetSequencePointForInstructiion(md.DebugInformation.SequencePoints, instr);
                    logger.Error(e, sq);
                }

                if (!weavedMethod)
                {
                    propertySiteProcessor.ProcessInstruction(md, ref instr);
                }

                instr = instr.Next;
            }
        }
    }
}
