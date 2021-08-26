using System;
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

    internal class SerializeFunctionException : WeaverException
    {
        public SerializeFunctionException(string message, MemberReference memberReference) : base(message, memberReference, null) { }
    }

    internal class NetworkBehaviourException : WeaverException
    {
        public NetworkBehaviourException(string message, TypeDefinition type) : base(message, type, null) { }
    }

    internal class SyncVarException : WeaverException
    {
        public SyncVarException(string message, MemberReference memberReference) : base(message, memberReference, null) { }
    }
}


namespace Mirage.Weaver.SyncVars
{
    internal class HookMethodException : SyncVarException
    {
        public HookMethodException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class BitCountException : SyncVarException
    {
        public BitCountException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class ZigZagException : SyncVarException
    {
        public ZigZagException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class BitCountFromRangeException : SyncVarException
    {
        public BitCountFromRangeException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class FloatPackException : SyncVarException
    {
        public FloatPackException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class Vector3PackException : SyncVarException
    {
        public Vector3PackException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class Vector2PackException : SyncVarException
    {
        public Vector2PackException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
    internal class QuaternionPackException : SyncVarException
    {
        public QuaternionPackException(string message, MemberReference memberReference) : base(message, memberReference) { }
    }
}
