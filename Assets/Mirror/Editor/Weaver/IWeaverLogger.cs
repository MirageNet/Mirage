using Mono.Cecil;

namespace Mirror.Weaver
{
    public interface IWeaverLogger
    {
        void Error(string msg);
        void Error(string message, MemberReference mr);
        void Error(string message, MethodDefinition md);
        void Warning(string msg);
        void Warning(string message, MemberReference mr);
    }
}