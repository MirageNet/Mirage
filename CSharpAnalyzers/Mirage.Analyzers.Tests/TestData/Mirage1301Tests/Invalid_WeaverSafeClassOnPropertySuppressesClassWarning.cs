using Mirage;
using System.Threading;

[NetworkMessage]
public struct StartSessionMessage
{
    [WeaverSafeClass]
    public Thread {|#0:ExecutionThread|} { get; set; }
}
