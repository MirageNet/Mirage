using System.Linq;
using Mirage.CodeGen;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Thrown when can't generate read or write for a type
    /// </summary>
    internal class SerializeFunctionException : WeaverException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Reason method could not be generated</param>
        /// <param name="typeRef">Type that read or write could not be generated for</param>
        public SerializeFunctionException(string message, TypeReference typeRef) : base(message, typeRef, null) { }
    }

    internal class NetworkBehaviourException : WeaverException
    {
        public NetworkBehaviourException(string message, TypeDefinition type, SequencePoint sequencePoint = null) : base(message, type, sequencePoint) { }
        public NetworkBehaviourException(string message, MemberReference memberReference, SequencePoint sequencePoint = null) : base(message, memberReference, sequencePoint) { }
    }

    internal class SyncVarException : WeaverException
    {
        public SyncVarException(string message, MemberReference memberReference) : base(message, memberReference, null) { }
        public SyncVarException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message, memberReference, sequencePoint) { }
    }

    internal class RpcException : WeaverException
    {
        public RpcException(string message, MethodReference rpcMethod) : base(message, rpcMethod, rpcMethod.Resolve().DebugInformation.SequencePoints.FirstOrDefault()) { }
    }
}


namespace Mirage.Weaver.SyncVars
{
    internal class HookMethodException : SyncVarException
    {
        public HookMethodException(string message, MemberReference memberReference) : base(message, memberReference) { }
        public HookMethodException(string message, MemberReference memberReference, MethodDefinition method) : base(message, memberReference, method.GetFirstSequencePoint()) { }
    }
}

namespace Mirage.Weaver.Serialization
{
    internal abstract class ValueSerializerException : WeaverException
    {
        protected ValueSerializerException(string message) : base(message, null, null) { }
    }

    internal class BitCountException : ValueSerializerException
    {
        public BitCountException(string message) : base(message) { }
    }
    internal class VarIntException : ValueSerializerException
    {
        public VarIntException(string message) : base(message) { }
    }
    internal class VarIntBlocksException : ValueSerializerException
    {
        public VarIntBlocksException(string message) : base(message) { }
    }
    internal class ZigZagException : ValueSerializerException
    {
        public ZigZagException(string message) : base(message) { }
    }
    internal class BitCountFromRangeException : ValueSerializerException
    {
        public BitCountFromRangeException(string message) : base(message) { }
    }
    internal class FloatPackException : ValueSerializerException
    {
        public FloatPackException(string message) : base(message) { }
    }
    internal class Vector3PackException : ValueSerializerException
    {
        public Vector3PackException(string message) : base(message) { }
    }
    internal class Vector2PackException : ValueSerializerException
    {
        public Vector2PackException(string message) : base(message) { }
    }
    internal class QuaternionPackException : ValueSerializerException
    {
        public QuaternionPackException(string message) : base(message) { }
    }
}
