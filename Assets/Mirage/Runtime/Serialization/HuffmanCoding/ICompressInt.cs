/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

namespace Mirage.Serialization.HuffmanCoding
{
    public interface ICompressInt
    {
        void Write(NetworkWriter writer, uint value);
        uint Read(NetworkReader reader);
    }

    public static class ICompressIntExtension
    {
        public static void WriteSigned(this ICompressInt compress, NetworkWriter writer, int value)
        {
            var zig = ZigZag.Encode(value);
            compress.Write(writer, zig);
        }
        public static int ReadSigned(this ICompressInt compress, NetworkReader reader)
        {
            var zig = compress.Read(reader);
            return ZigZag.Decode(zig);
        }
    }


}
