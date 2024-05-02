using System.IO;
using Vortice.Direct3D;

namespace MNL
{
    public class NiColorExtraData : NiExtraData
    {
        public Vector4 Data;

        public NiColorExtraData(NiFile file, BinaryReader reader)
            : base(file, reader)
        {
            Data = reader.ReadColor4();
        }
    }
}
