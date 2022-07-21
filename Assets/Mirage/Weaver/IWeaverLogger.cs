using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    // todo rename this to IWeaverErrors because it isn't really a logger
    public interface IWeaverLogger
    {
        /// <summary>
        /// Should error message show stack trace of errror. Mostly used for debugging weaver
        /// </summary>
        bool EnableTrace { get; }

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
    }
}
