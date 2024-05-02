using System.Collections.Generic;
using System.IO;
using Vortice.Direct3D;
using Microsoft.DirectX.Direct3D;
using MNL;
namespace HMapEdit.Tools
{
	public class NIFModel
	{
		public readonly string filename;
		private readonly NiFile _niFile;
		private static Dictionary<string, NiFile> _cachedFiles = new Dictionary<string, NiFile>();

		/// <summary>
		/// Loads a nif model
		/// </summary>
		/// <param name="nif"></param>
		public NIFModel(Stream nif, string filename, bool useCache = true)
		{
			this.filename = filename;
			if (!useCache)
			{
				using (var r = new BinaryReader(nif))
					_niFile = new NiFile(r, filename);
				return;
			}

			_cachedFiles.TryGetValue(filename, out _niFile);
			if (_niFile == null)
			{
				using (var r = new BinaryReader(nif))
					_niFile = new NiFile(r, filename);
				_cachedFiles.Add(filename, _niFile);
			}
		}

		public void DxInit(Device device)
		{
			if (_niFile != null)
				_niFile.DxInit(device);
		}
		public void DxDeinit()
		{
			if (_niFile != null)
				_niFile.DxDeinit();
		}

		/// <summary>
		/// Renders the model
		/// </summary>
		public void Render(LocalTextures localTextures, Effect effect, ref Matrix world)
		{
			if (_niFile == null)
				return;
			var position = Vector3.Transform(Vector3.Empty, world);
			_niFile.Render(localTextures, effect, new Vector3(position.X, position.Y, position.Z));
		}

		/// <summary>
		/// Checks whether the ray intersects the mesh or not
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public bool Intersect(Vector3 src, Vector3 dir)
		{
			if (_niFile == null)
				return false;
			//return _niFile.Intersect(src, dir);
			return false;
		}
	}
}