using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.CodeGen
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
    public class WeaverException : Exception
    {
        public readonly SequencePoint SequencePoint;
        public readonly MemberReference MemberReference;

        public WeaverException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message)
        {
            SequencePoint = sequencePoint;
            MemberReference = memberReference;
        }
    }
}
