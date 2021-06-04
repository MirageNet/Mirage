using UnityEngine;

namespace Mirage.Serialization
{
    public static class UnityTypesExtensions
    {
        public static void WriteVector2(this NetworkWriter writer, Vector2 value)
        {
            writer.WriteSingle(value.x);
            writer.WriteSingle(value.y);
        }

        public static void WriteVector3(this NetworkWriter writer, Vector3 value)
        {
            writer.WriteSingle(value.x);
            writer.WriteSingle(value.y);
            writer.WriteSingle(value.z);
        }

        public static void WriteVector4(this NetworkWriter writer, Vector4 value)
        {
            writer.WriteSingle(value.x);
            writer.WriteSingle(value.y);
            writer.WriteSingle(value.z);
            writer.WriteSingle(value.w);
        }

        public static void WriteVector2Int(this NetworkWriter writer, Vector2Int value)
        {
            writer.WritePackedInt32(value.x);
            writer.WritePackedInt32(value.y);
        }

        public static void WriteVector3Int(this NetworkWriter writer, Vector3Int value)
        {
            writer.WritePackedInt32(value.x);
            writer.WritePackedInt32(value.y);
            writer.WritePackedInt32(value.z);
        }

        public static void WriteColor(this NetworkWriter writer, Color value)
        {
            writer.WriteSingle(value.r);
            writer.WriteSingle(value.g);
            writer.WriteSingle(value.b);
            writer.WriteSingle(value.a);
        }

        public static void WriteColor32(this NetworkWriter writer, Color32 value)
        {
            writer.WriteByte(value.r);
            writer.WriteByte(value.g);
            writer.WriteByte(value.b);
            writer.WriteByte(value.a);
        }

        public static void WriteQuaternion(this NetworkWriter writer, Quaternion value)
        {
            writer.WriteSingle(value.x);
            writer.WriteSingle(value.y);
            writer.WriteSingle(value.z);
            writer.WriteSingle(value.w);
        }

        public static void WriteRect(this NetworkWriter writer, Rect value)
        {
            writer.WriteSingle(value.xMin);
            writer.WriteSingle(value.yMin);
            writer.WriteSingle(value.width);
            writer.WriteSingle(value.height);
        }

        public static void WritePlane(this NetworkWriter writer, Plane value)
        {
            writer.WriteVector3(value.normal);
            writer.WriteSingle(value.distance);
        }

        public static void WriteRay(this NetworkWriter writer, Ray value)
        {
            writer.WriteVector3(value.origin);
            writer.WriteVector3(value.direction);
        }

        public static void WriteMatrix4X4(this NetworkWriter writer, Matrix4x4 value)
        {
            writer.WriteSingle(value.m00);
            writer.WriteSingle(value.m01);
            writer.WriteSingle(value.m02);
            writer.WriteSingle(value.m03);
            writer.WriteSingle(value.m10);
            writer.WriteSingle(value.m11);
            writer.WriteSingle(value.m12);
            writer.WriteSingle(value.m13);
            writer.WriteSingle(value.m20);
            writer.WriteSingle(value.m21);
            writer.WriteSingle(value.m22);
            writer.WriteSingle(value.m23);
            writer.WriteSingle(value.m30);
            writer.WriteSingle(value.m31);
            writer.WriteSingle(value.m32);
            writer.WriteSingle(value.m33);
        }


        public static Vector2 ReadVector2(this NetworkReader reader) => new Vector2(reader.ReadSingle(), reader.ReadSingle());
        public static Vector3 ReadVector3(this NetworkReader reader) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        public static Vector4 ReadVector4(this NetworkReader reader) => new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        public static Vector2Int ReadVector2Int(this NetworkReader reader) => new Vector2Int(reader.ReadPackedInt32(), reader.ReadPackedInt32());
        public static Vector3Int ReadVector3Int(this NetworkReader reader) => new Vector3Int(reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadPackedInt32());
        public static Color ReadColor(this NetworkReader reader) => new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        public static Color32 ReadColor32(this NetworkReader reader) => new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        public static Quaternion ReadQuaternion(this NetworkReader reader) => new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        public static Rect ReadRect(this NetworkReader reader) => new Rect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        public static Plane ReadPlane(this NetworkReader reader) => new Plane(reader.ReadVector3(), reader.ReadSingle());
        public static Ray ReadRay(this NetworkReader reader) => new Ray(reader.ReadVector3(), reader.ReadVector3());
        public static Matrix4x4 ReadMatrix4x4(this NetworkReader reader)
        {
            return new Matrix4x4
            {
                m00 = reader.ReadSingle(),
                m01 = reader.ReadSingle(),
                m02 = reader.ReadSingle(),
                m03 = reader.ReadSingle(),
                m10 = reader.ReadSingle(),
                m11 = reader.ReadSingle(),
                m12 = reader.ReadSingle(),
                m13 = reader.ReadSingle(),
                m20 = reader.ReadSingle(),
                m21 = reader.ReadSingle(),
                m22 = reader.ReadSingle(),
                m23 = reader.ReadSingle(),
                m30 = reader.ReadSingle(),
                m31 = reader.ReadSingle(),
                m32 = reader.ReadSingle(),
                m33 = reader.ReadSingle()
            };
        }
    }
}
