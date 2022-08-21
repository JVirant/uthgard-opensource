using Microsoft.DirectX;
using System.IO;

namespace MNL
{
    public static class ReaderExtensions
    {
        public static float[] ReadFloatArray(this BinaryReader reader, int length)
        {
            var floats = new float[length];
            for (var i = 0; i < floats.Length; i++)
                floats[i] = reader.ReadSingle();

            return floats;
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
        {
            var result = new uint[length];
            for (var i = 0; i < result.Length; i++)
                result[i] = reader.ReadUInt32();

            return result;
        }

        public static ushort[] ReadUInt16Array(this BinaryReader reader, int length)
        {
            var result = new ushort[length];
            for (var i = 0; i < result.Length; i++)
                result[i] = reader.ReadUInt16();

            return result;
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadColor3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadColor4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadColor4Byte(this BinaryReader reader)
        {
            return new Vector4(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f);
        }

        public static Matrix ReadMatrix33(this BinaryReader reader)
        {
            var result = Matrix.Identity;
            result.M11 = reader.ReadSingle();
            result.M21 = reader.ReadSingle();
            result.M31 = reader.ReadSingle();
            // result.M41 = reader.ReadSingle();
            result.M12 = reader.ReadSingle();
            result.M22 = reader.ReadSingle();
            result.M32 = reader.ReadSingle();
            // result.M42 = reader.ReadSingle();
            result.M13 = reader.ReadSingle();
            result.M23 = reader.ReadSingle();
            result.M33 = reader.ReadSingle();
            // result.M43 = reader.ReadSingle();
            //result.M14 = reader.ReadSingle();
            //result.M24 = reader.ReadSingle();
            //result.M34 = reader.ReadSingle();
            //result.M44 = reader.ReadSingle();
            return result;
        }
    }
}
