using System.Collections.Generic;
using Mirage.CodeGen;
using Mirage.Weaver.NetworkBehaviours;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.Weaver
{
    public enum RemoteCallType
    {
        ServerRpc,
        ClientRpc
    }

    public enum ReturnType
    {
        Void,
        UniTask,
    }

    /// <summary>
    /// processes SyncVars, Cmds, Rpcs, etc. of NetworkBehaviours
    /// </summary>
    internal class NetworkBehaviourProcessor
    {
        private readonly TypeDefinition netBehaviourSubclass;
        private readonly IWeaverLogger logger;
        private readonly ServerRpcProcessor serverRpcProcessor;
        private readonly ClientRpcProcessor clientRpcProcessor;
        private readonly SyncVarProcessor syncVarProcessor;
        private readonly SyncObjectProcessor syncObjectProcessor;
        private readonly ConstFieldTracker rpcCounter;

        public NetworkBehaviourProcessor(TypeDefinition td, Readers readers, Writers writers, PropertySiteProcessor propertySiteProcessor, IWeaverLogger logger)
        {
            Weaver.DebugLog(td, "NetworkBehaviourProcessor");
            netBehaviourSubclass = td;
            this.logger = logger;
            serverRpcProcessor = new ServerRpcProcessor(netBehaviourSubclass.Module, readers, writers, logger);
            clientRpcProcessor = new ClientRpcProcessor(netBehaviourSubclass.Module, readers, writers, logger);
            syncVarProcessor = new SyncVarProcessor(netBehaviourSubclass.Module, readers, writers, propertySiteProcessor);
            syncObjectProcessor = new SyncObjectProcessor(readers, writers, logger);

            // no max for rpcs, index is sent as var int, so more rpc just means bigger header size (still smaller than 4 byte hash)
            rpcCounter = new ConstFieldTracker("RPC_COUNT", td, int.MaxValue, "Rpc");
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
                var versionMethod = td.AddMethod(ProcessedFunctionName, MethodAttributes.Private);
                var worker = versionMethod.Body.GetILProcessor();
                worker.Append(worker.Create(OpCodes.Ret));
            }
        }
        #endregion

        private void RegisterRpcs(List<RpcMethod> rpcs)
        {
            Weaver.DebugLog(netBehaviourSubclass, "Set const RPC Count");
            SetRpcCount(rpcs.Count);

            // if there are no rpcs then we dont need to override method
            if (rpcs.Count == 0)
                return;

            Weaver.DebugLog(netBehaviourSubclass, "Override RegisterRPC");

            var helper = new RegisterRpcHelper(netBehaviourSubclass.Module, netBehaviourSubclass);
            if (helper.HasManualOverride())
                throw new RpcException($"{helper.MethodName} should not have a manual override", helper.GetManualOverride());

            helper.AddMethod();

            RegisterRpc.RegisterAll(helper.Worker, rpcs);

            helper.Worker.Emit(OpCodes.Ret);
        }

        private void SetRpcCount(int count)
        {
            // set const so that child classes know count of base classes
            rpcCounter.Set(count);

            // override virtual method so returns total
            var method = netBehaviourSubclass.AddMethod(nameof(NetworkBehaviour.GetRpcCount), MethodAttributes.Virtual | MethodAttributes.Family, typeof(int));
            var worker = method.Body.GetILProcessor();
            // write count of base+current so that `GetInBase` call will return total
            worker.Emit(OpCodes.Ldc_I4, rpcCounter.GetInBase() + count);
            worker.Emit(OpCodes.Ret);
        }

        private void ProcessRpcs()
        {
            // copy the list of methods because we will be adding methods in the loop
            var methods = new List<MethodDefinition>(netBehaviourSubclass.Methods);

            var rpcs = new List<RpcMethod>();

            var index = rpcCounter.GetInBase();
            foreach (var md in methods)
            {
                try
                {
                    var rpc = CheckAndProcessRpc(md, index);
                    if (rpc != null)
                    {
                        // increment only if rpc was count
                        index++;
                        rpcs.Add(rpc);
                    }
                }
                catch (RpcException e)
                {
                    logger.Error(e);
                }
            }

            RegisterRpcs(rpcs);
        }

        private RpcMethod CheckAndProcessRpc(MethodDefinition md, int index)
        {
            if (md.TryGetCustomAttribute<ServerRpcAttribute>(out var serverAttribute))
            {
                if (md.HasCustomAttribute<ClientRpcAttribute>()) throw new RpcException("Method should not have both ServerRpc and ClientRpc", md);

                return serverRpcProcessor.ProcessRpc(md, serverAttribute, index);
            }
            else if (md.TryGetCustomAttribute<ClientRpcAttribute>(out var clientAttribute))
            {
                return clientRpcProcessor.ProcessRpc(md, clientAttribute, index);
            }
            return null;
        }
    }
}
