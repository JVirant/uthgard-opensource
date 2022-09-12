using Microsoft.DirectX.Direct3D;
using System;
using System.IO;

namespace MNL
{
	public class NiObject
	{
		public NiFile File;

		// conv. function :)
		public eNifVersion Version
		{
			get { return File.Version; }
		}

		public NiObject(NiFile file, BinaryReader reader)
		{
			File = file;
		}

		public virtual void DxInit(Device device) { }
		public virtual void DxDeinit() { }
		public virtual void Render(Device device) { }
	}
}
