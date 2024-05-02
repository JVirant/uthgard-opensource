using System;
using System.IO;
using Vortice.Direct3D;

namespace MNL
{
    public class Color4Key : BaseKey
    {
        public float Time = 0f;
        public Vector4 Value;
        public Vector4 Forward;
        public Vector4 Backward;

        public Color4Key(BinaryReader reader, eKeyType type) : base(reader, type)
        {
            Time = reader.ReadSingle();
            if (type < eKeyType.LINEAR_KEY || type > eKeyType.TBC_KEY)
                throw new Exception("Invalid eKeyType!");

            Value = reader.ReadColor4();

            if (type == eKeyType.QUADRATIC_KEY)
            {
                Forward = reader.ReadColor4();
                Backward = reader.ReadColor4();
            }
        }
    }
}
