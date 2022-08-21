using System;
using System.IO;
using System.Linq;
using HMapEdit.NiLib;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using MNL;
namespace HMapEdit.Tools
{
	/// <summary>
	/// SlimDX Nif Model
	/// </summary>
	public class NIFModel
	{
		private readonly Texture _objTexture;
		private readonly OBJModel _objModel;
		private readonly NiFile _niFile;

		/// <summary>
		/// Loads a nif model
		/// </summary>
		/// <param name="nif"></param>
		public NIFModel(Stream nif, string filename)
		{
			_objTexture = LocalTextures.Get("__OBJSOLID__", false);
			using (var r = new BinaryReader(nif))
			{
				_niFile = new NiFile(r, filename);
			}
			var obj = Ni2Obj.ToObj(_niFile);
			using (var ms = new MemoryStream())
			{
				obj.Save(new StreamWriter(ms));

#if DEBUG
				Directory.CreateDirectory("extracted/" + Path.GetDirectoryName(filename));
				File.WriteAllBytes("extracted/" + filename + ".obj", ms.ToArray());
#endif

				ms.Position = 0;
				_objModel = OBJLoader.Load(new StreamReader(ms), filename);
			}
		}

		/// <summary>
		/// Renders the model
		/// </summary>
		public void Render(Device device, Effect effect)
		{
			if (device != null && Program.CONFIG.ShowRawNIFs && _niFile != null)
				_niFile.Render(device, effect);
			else if (_objModel != null)
			{
				device.SetTexture(0, _objTexture);
				_objModel.Render();
			}
		}

		public void RenderWireframe(Device device)
		{
			device.RenderState.FillMode = FillMode.WireFrame;
			if (_objModel != null)
				_objModel.Render();
			device.RenderState.FillMode = Program.CONFIG.FillMode;
		}

		/// <summary>
		/// Checks whether the ray intersects the mesh or not
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public bool Intersect(Vector3 src, Vector3 dir)
		{
			return _objModel == null ? false : _objModel.Intersect(src, dir);
		}
	}
}