using System.IO;

namespace MNL
{
	public class NiShadeProperty : NiProperty
	{
		public ushort flags = 0;

		public NiShadeProperty(NiFile file, BinaryReader reader)
			: base(file, reader)
		{
			flags = reader.ReadUInt16();
		}
	}
}
