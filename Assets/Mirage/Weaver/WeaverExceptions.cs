using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    internal abstract class WeaverException : Exception
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
}
