using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.CodeGen
{
    public static class LoggerExtensions
    {
        public static void Error(this IWeaverLogger logger, WeaverException exception)
        {
            var message = exception.Message;
            if (logger.EnableTrace)
                message += "\n" + exception.ToString();

            logger.Error(message, exception.MemberReference, exception.SequencePoint);
        }

        public static void Error(this IWeaverLogger logger, WeaverException exception, SequencePoint sequencePoint)
        {
            var message = exception.Message;
            if (logger.EnableTrace)
                message += "\n" + exception.ToString();

            logger.Error(message, exception.MemberReference, sequencePoint);
        }

        public static SequencePoint GetSequencePoint(this MethodDefinition method, Instruction instruction)
        {
            var sequencePoint = method.DebugInformation.GetSequencePoint(instruction);
            return sequencePoint;
        }

        public static SequencePoint GetFirstSequencePoint(this MethodDefinition method)
        {
            var firstInstruction = method.Body.Instructions.First();
            var sequencePoint = method.DebugInformation.GetSequencePoint(firstInstruction);
            return sequencePoint;
        }
    }
}
