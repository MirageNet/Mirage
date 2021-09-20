using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Mirage.Weaver
{
    public enum RemoteCallType
    {
        ServerRpc,
        ClientRpc
    }

    /// <summary>
    /// processes SyncVars, Cmds, Rpcs, etc. of NetworkBehaviours
    /// </summary>
    class NetworkBehaviourProcessor
    {
        readonly TypeDefinition netBehaviourSubclass;
        private readonly IWeaverLogger logger;
        readonly ServerRpcProcessor serverRpcProcessor;
        readonly ClientRpcProcessor clientRpcProcessor;
        readonly SyncVarProcessor syncVarProcessor;
        readonly SyncObjectProcessor syncObjectProcessor;

        public NetworkBehaviourProcessor(TypeDefinition td, Readers readers, Writers writers, PropertySiteProcessor propertySiteProcessor, IWeaverLogger logger)
        {
            Weaver.DebugLog(td, "NetworkBehaviourProcessor");
            netBehaviourSubclass = td;
            this.logger = logger;
            serverRpcProcessor = new ServerRpcProcessor(netBehaviourSubclass.Module, readers, writers, logger);
            clientRpcProcessor = new ClientRpcProcessor(netBehaviourSubclass.Module, readers, writers, logger);
            syncVarProcessor = new SyncVarProcessor(netBehaviourSubclass.Module, readers, writers, propertySiteProcessor);
            syncObjectProcessor = new SyncObjectProcessor(readers, writers, logger);
        }

        // return true if modified
        public bool Process()
        {
            // only process once
            if (WasProcessed(netBehaviourSubclass))
            {
                return false;
            }
            Weaver.DebugLog(netBehaviourSubclass, $"Found NetworkBehaviour {netBehaviourSubclass.FullName}");

            Weaver.DebugLog(netBehaviourSubclass, "Process Start");
            MarkAsProcessed(netBehaviourSubclass);

            try
            {
                syncVarProcessor.ProcessSyncVars(netBehaviourSubclass, logger);
            }
            catch (NetworkBehaviourException e)
            {
                logger.Error(e);
            }

            syncObjectProcessor.ProcessSyncObjects(netBehaviourSubclass);

            ProcessRpcs();

            Weaver.DebugLog(netBehaviourSubclass, "Process Done");
            return true;
        }

        #region mark / check type as processed
        public const string ProcessedFunctionName = "MirageProcessed";

        // by adding an empty MirageProcessed() function
        public static bool WasProcessed(TypeDefinition td)
        {
            return td.GetMethod(ProcessedFunctionName) != null;
        }

        public static void MarkAsProcessed(TypeDefinition td)
        {
            if (!WasProcessed(td))
            {
                MethodDefinition versionMethod = td.AddMethod(ProcessedFunctionName, MethodAttributes.Private);
                ILProcessor worker = versionMethod.Body.GetILProcessor();
                worker.Append(worker.Create(OpCodes.Ret));
            }
        }
        #endregion

        void RegisterRpcs()
        {
            Weaver.DebugLog(netBehaviourSubclass, "  GenerateConstants ");

            AddToStaticConstructor(netBehaviourSubclass, (worker) =>
            {
                serverRpcProcessor.RegisterServerRpcs(worker);
                clientRpcProcessor.RegisterClientRpcs(worker);
            });
        }

        void ProcessRpcs()
        {
            var names = new HashSet<string>();

            // copy the list of methods because we will be adding methods in the loop
            var methods = new List<MethodDefinition>(netBehaviourSubclass.Methods);
            // find ServerRpc and RPC functions
            foreach (MethodDefinition md in methods)
            {
                bool isRpc = CheckAndProcessRpc(md);

                if (isRpc)
                {
                    if (names.Contains(md.Name))
                    {
                        logger.Error($"Duplicate Rpc name {md.Name}", md);
                    }
                    names.Add(md.Name);
                }
            }

            RegisterRpcs();
        }

        private bool CheckAndProcessRpc(MethodDefinition md)
        {
            try
            {
                if (md.TryGetCustomAttribute<ServerRpcAttribute>(out CustomAttribute serverAttribute))
                {
                    if (md.HasCustomAttribute<ClientRpcAttribute>()) throw new RpcException("Method should not have both ServerRpc and ClientRpc", md);

                    // todo make processRpc return the found Rpc instead of saving it to hidden list
                    serverRpcProcessor.ProcessRpc(md, serverAttribute);
                    return true;
                }
                else if (md.TryGetCustomAttribute<ClientRpcAttribute>(out CustomAttribute clientAttribute))
                {
                    // todo make processRpc return the found Rpc instead of saving it to hidden list
                    clientRpcProcessor.ProcessRpc(md, clientAttribute);
                    return true;
                }
            }
            catch (RpcException e)
            {
                logger.Error(e);
            }

            return false;
        }

        /// <summary>
        /// Adds code to static Constructor
        /// <para>
        /// If Constructor is missing a new one will be created
        /// </para>
        /// </summary>
        /// <param name="body">code to write</param>
        public static void AddToStaticConstructor(TypeDefinition typeDefinition, Action<ILProcessor> body)
        {
            MethodDefinition cctor = typeDefinition.GetMethod(".cctor");
            if (cctor != null)
            {
                // remove the return opcode from end of function. will add our own later.
                if (cctor.Body.Instructions.Count != 0)
                {
                    Instruction retInstr = cctor.Body.Instructions[cctor.Body.Instructions.Count - 1];
                    if (retInstr.OpCode == OpCodes.Ret)
                    {
                        cctor.Body.Instructions.RemoveAt(cctor.Body.Instructions.Count - 1);
                    }
                    else
                    {
                        throw new NetworkBehaviourException($"{typeDefinition.Name} has invalid static constructor", cctor, cctor.GetSequencePoint(retInstr));
                    }
                }
            }
            else
            {
                // make one!
                cctor = typeDefinition.AddMethod(".cctor", MethodAttributes.Private |
                        MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName |
                        MethodAttributes.RTSpecialName |
                        MethodAttributes.Static);
            }

            ILProcessor worker = cctor.Body.GetILProcessor();

            // add new code to bottom of constructor
            // todo should we be adding new code to top of function instead? incase user has early return in custom constructor?
            body.Invoke(worker);

            // re-add return bececause we removed it earlier
            worker.Append(worker.Create(OpCodes.Ret));

            // in case class had no cctor, it might have BeforeFieldInit, so injected cctor would be called too late
            typeDefinition.Attributes &= ~TypeAttributes.BeforeFieldInit;
        }
    }
}
