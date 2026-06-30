using Mirage;
using System.Threading;

[NetworkMessage]
public struct MessageWithPrivateField
{
    private Thread {|#0:executionThread|};
}
