using System;
using System.Runtime.InteropServices;

namespace Mirage.Serialization
{
    public static class SystemTypesExtensions
    {
        // todo benchmark converters 
        /// <summary>
        /// Converts between uint and float without allocations
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntFloat
        {
            [FieldOffset(0)]
            public float floatValue;

            [FieldOffset(0)]
            public uint intValue;
        }

        /// <summary>
        /// Converts between ulong and double without allocations
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntDouble
        {
            [FieldOffset(0)]
            public double doubleValue;

            [FieldOffset(0)]
            public ulong longValue;
        }

        /// <summary>
        /// Converts between ulong and decimal without allocations
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntDecimal
        {
            [FieldOffset(0)]
            public ulong longValue1;

            [FieldOffset(8)]
            public ulong longValue2;

            [FieldOffset(0)]
            public decimal decimalValue;
        }

        public static void WriteByteExtension(this NetworkWriter writer, byte value) => writer.WriteByte(value);

        public static void WriteSByteExtension(this NetworkWriter writer, sbyte value) => writer.WriteSByte(value);

        public static void WriteChar(this NetworkWriter writer, char value) => writer.WriteUInt16(value);

        public static void WriteBooleanExtension(this NetworkWriter writer, bool value) => writer.WriteBoolean(value);

        public static void WriteUInt16Extension(this NetworkWriter writer, ushort value)
        {
            writer.WriteUInt16(value);
        }

        public static void WriteInt16Extension(this NetworkWriter writer, short value) => writer.WriteInt16(value);

        public static void WriteSingleConverter(this NetworkWriter writer, float value)
        {
            var converter = new UIntFloat
            {
                floatValue = value
            };
            writer.WriteUInt32(converter.intValue);
        }
        public static void WriteDoubleConverter(this NetworkWriter writer, double value)
        {
            var converter = new UIntDouble
            {
                doubleValue = value
            };
            writer.WriteUInt64(converter.longValue);
        }
        public static void WriteDecimalConverter(this NetworkWriter writer, decimal value)
        {
            // the only way to read it without allocations is to both read and
            // write it with the FloatConverter (which is not binary compatible
            // to writer.Write(decimal), hence why we use it here too)
            var converter = new UIntDecimal
            {
                decimalValue = value
            };
            writer.WriteUInt64(converter.longValue1);
            writer.WriteUInt64(converter.longValue2);
        }

        public static void WriteGuid(this NetworkWriter writer, Guid value)
        {
            var data = value.ToByteArray();
            writer.WriteBytes(data, 0, data.Length);
        }

        [WeaverSerializeCollection]
        public static void WriteNullable<T>(this NetworkWriter writer, T? nullable) where T : struct
        {
            var hasValue = nullable.HasValue;
            writer.WriteBoolean(hasValue);
            if (hasValue)
            {
                writer.Write(nullable.Value);
            }
        }




        public static byte ReadByteExtension(this NetworkReader reader) => reader.ReadByte();
        public static sbyte ReadSByteExtension(this NetworkReader reader) => reader.ReadSByte();
        public static char ReadChar(this NetworkReader reader) => (char)reader.ReadUInt16();
        public static bool ReadBooleanExtension(this NetworkReader reader) => reader.ReadBoolean();
        public static short ReadInt16Extension(this NetworkReader reader) => reader.ReadInt16();
        public static ushort ReadUInt16Extension(this NetworkReader reader) => reader.ReadUInt16();
        public static float ReadSingleConverter(this NetworkReader reader)
        {
            var converter = new UIntFloat
            {
                intValue = reader.ReadUInt32()
            };
            return converter.floatValue;
        }
        public static double ReadDoubleConverter(this NetworkReader reader)
        {
            var converter = new UIntDouble
            {
                longValue = reader.ReadUInt64()
            };
            return converter.doubleValue;
        }
        public static decimal ReadDecimalConverter(this NetworkReader reader)
        {
            var converter = new UIntDecimal
            {
                longValue1 = reader.ReadUInt64(),
                longValue2 = reader.ReadUInt64()
            };
            return converter.decimalValue;
        }
        public static Guid ReadGuid(this NetworkReader reader) => new Guid(reader.ReadBytes(16));

        [WeaverSerializeCollection]
        public static T? ReadNullable<T>(this NetworkReader reader) where T : struct
        {
            var hasValue = reader.ReadBoolean();
            if (hasValue)
            {
                return reader.Read<T>();
            }
            else
            {
                return null;
            }
        }
    }
}
