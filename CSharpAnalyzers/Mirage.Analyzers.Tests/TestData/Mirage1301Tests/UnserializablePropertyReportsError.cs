using Mirage;
using System.Threading;

[NetworkMessage]
public struct StartSessionMessage
{
    public Thread {|#0:ExecutionThread|} { get; set; }
}
