using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public abstract class WeaverException : Exception
    {
        public readonly SequencePoint SequencePoint;
        public readonly MemberReference MemberReference;

        protected WeaverException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message)
        {
            SequencePoint = sequencePoint;
            MemberReference = memberReference;
        }
    }

    public class SerializeFunctionException : WeaverException
    {
        public SerializeFunctionException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message, memberReference, sequencePoint) { }
    }
}
