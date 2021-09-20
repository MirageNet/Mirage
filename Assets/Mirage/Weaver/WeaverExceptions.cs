using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Weaver Exception should be thrown when there is a problem with the users code that they should fix
    /// <para>
    ///     For example, if the user uses an unsupported type a WeaverException should be thrown with a
    ///     message explaining the problem, and the MemberReference to help the user find the issue
    /// </para>
    /// <para>
    ///     For Exception that are internally to weaver (eg weaver didn't work right) and normal Exception should be thrown
    /// </para>
    /// </summary>
    // should be caught within weaver and returned to user using DiagnosticMessage
#pragma warning disable S3871 // Exception types should be "public"
    internal abstract class WeaverException : Exception
#pragma warning restore S3871 // Exception types should be "public"
    {
        public readonly SequencePoint SequencePoint;
        public readonly MemberReference MemberReference;

        protected WeaverException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message)
        {
            SequencePoint = sequencePoint;
            MemberReference = memberReference;
        }
    }

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
