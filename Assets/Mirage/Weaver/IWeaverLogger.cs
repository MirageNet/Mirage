using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public interface IWeaverLogger
    {
        void Error(string message);
        void Error(string message, MemberReference mr);
        void Error(string message, MemberReference mr, SequencePoint sequencePoint);
        void Error(string message, MethodDefinition md);

        void Warning(string message);
        void Warning(string message, MemberReference mr);
        void Warning(string message, MemberReference mr, SequencePoint sequencePoint);
        void Warning(string message, MethodDefinition md);
    }

    internal static class WeaverLoggerExtensions
    {
        public static void Error(this IWeaverLogger logger, WeaverException exception)
        {
            logger.Error(exception.Message, exception.MemberReference, exception.SequencePoint);
        }

        public static void Error(this IWeaverLogger logger, WeaverException exception, SequencePoint sequencePoint)
        {
            logger.Error(exception.Message, exception.MemberReference, sequencePoint);
        }
    }
}
