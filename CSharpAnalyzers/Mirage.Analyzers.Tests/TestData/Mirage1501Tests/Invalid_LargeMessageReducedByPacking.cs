using Mirage;
using Mirage.Serialization;

[NetworkMessage]
public struct {|#0:PackedTestMessage|}
{
    [FloatPack(0.0f, 1)] // Should estimate size based on 1 bit instead of 4/8 bytes
    public float CompressedVal;
}
