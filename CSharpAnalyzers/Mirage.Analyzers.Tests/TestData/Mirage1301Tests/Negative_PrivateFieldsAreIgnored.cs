using Mirage;
using System.Threading;

[NetworkMessage]
public struct MessageWithPrivateField
{
    private Thread executionThread;
}
