using System.IO;
using Vortice.Direct3D;
using Microsoft.DirectX.Direct3D;

namespace MNL
{
	public class NiLODNode : NiSwitchNode
	{
		public Vector3 LODCenter;
		public LODRange[] LODLevels;
		public NiRef<NiLODData> LODLevelData;

		public NiLODNode(NiFile file, BinaryReader reader)
			: base(file, reader)
		{
			if (Version >= eNifVersion.VER_4_0_0_2
				&& Version <= eNifVersion.VER_10_0_1_0)
			{
				LODCenter = reader.ReadVector3();
			}

			if (Version <= eNifVersion.VER_10_0_1_0)
			{
				var numLODLevels = reader.ReadUInt32();
				LODLevels = new LODRange[numLODLevels];
				for (var i = 0; i < numLODLevels; i++)
					LODLevels[i] = new LODRange(file, reader);
			}

			if (Version >= eNifVersion.VER_10_0_1_0)
			{
				LODLevelData = new NiRef<NiLODData>(reader);
			}
		}

		public override void DxInit(Device device)
		{
			base.DxInit(device);
			if (LODLevelData?.IsValid() ?? false)
				LODLevelData.Object.DxInit(device);
		}
		public override void DxDeinit()
		{
			base.DxDeinit();
			if (LODLevelData?.IsValid() ?? false)
				LODLevelData.Object.DxDeinit();
		}
		public override void Render(Device device)
		{
			base.Render(device);
			if (LODLevelData?.IsValid() ?? false)
				LODLevelData.Object.Render(device);
		}
	}
}