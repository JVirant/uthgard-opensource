using Microsoft.DirectX.Direct3D;
using System.IO;

namespace MNL
{
	public class NiNode : NiAVObject
	{
		public NiRef<NiAVObject>[] Children;
		public NiRef<NiDynamicEffect>[] Effects;

		public NiNode(NiFile file, BinaryReader reader)
			: base(file, reader)
		{
			Children = new NiRef<NiAVObject>[reader.ReadUInt32()];
			for (var i = 0; i < Children.Length; i++)
			{
				Children[i] = new NiRef<NiAVObject>(reader.ReadUInt32());
			}
			Effects = new NiRef<NiDynamicEffect>[reader.ReadUInt32()];
			for (var i = 0; i < Effects.Length; i++)
			{
				Effects[i] = new NiRef<NiDynamicEffect>(reader.ReadUInt32());
			}
		}

		public override void DxInit(Device device)
		{
			base.DxInit(device);
			foreach (var child in Children)
				if (child.IsValid())
					child.Object.DxInit(device);
			foreach (var effect in Effects)
				if (effect.IsValid())
					effect.Object.DxInit(device);
		}
		public override void DxDeinit()
		{
			base.DxDeinit();
			foreach (var child in Children)
				if (child.IsValid())
					child.Object.DxDeinit();
			foreach (var effect in Effects)
				if (effect.IsValid())
					effect.Object.DxDeinit();
		}
		public override void Render(Device device)
		{
			base.Render(device);
			foreach (var child in Children)
				if (child.IsValid())
					child.Object.Render(device);
			foreach (var effect in Effects)
				if (effect.IsValid())
					effect.Object.Render(device);
		}
	}
}
