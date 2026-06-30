using Mirage;
using System.Threading;

[WeaverSafeClass]
public class SafeClassWithThread
{
    public Thread threadField;
}

[NetworkMessage]
public struct Message
{
    public SafeClassWithThread {|#0:safeClassField|};
}
