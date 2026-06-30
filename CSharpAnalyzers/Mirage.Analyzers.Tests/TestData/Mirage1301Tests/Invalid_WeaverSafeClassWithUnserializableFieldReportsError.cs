using Mirage;
using System.Threading;

[WeaverSafeClass]
public class SafeClassWithThread
{
    public Thread {|#0:threadField|};
}

[NetworkMessage]
public struct Message
{
    public SafeClassWithThread {|#1:safeClassField|};
}
