using Microsoft.DirectX.Direct3D;
using System.IO;

namespace MNL
{
	public class NiCollisionObject : NiObject
	{
		public NiRef<NiAVObject> Target;

		public NiCollisionObject(NiFile file, BinaryReader reader) : base(file, reader)
		{
			Target = new NiRef<NiAVObject>(reader);
		}

		public override void DxInit(Device device)
		{
			base.DxInit(device);
			if (Target.IsValid())
				Target.Object.DxInit(device);
		}
		public override void Render(Device device)
		{
			base.Render(device);
			if (Target.IsValid())
				Target.Object.Render(device);
		}
	}
}
