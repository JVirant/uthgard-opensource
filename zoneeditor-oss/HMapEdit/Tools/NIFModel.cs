using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using MNL;
namespace HMapEdit.Tools
{
	public class NIFModel
	{
		private readonly NiFile _niFile;
		private static Dictionary<string, NiFile> _cachedFiles = new Dictionary<string, NiFile>();

		/// <summary>
		/// Loads a nif model
		/// </summary>
		/// <param name="nif"></param>
		public NIFModel(Stream nif, string filename)
		{
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

		/// <summary>
		/// Renders the model
		/// </summary>
		public void Render(Device device, Effect effect)
		{
			if (_niFile != null)
				_niFile.Render(device, effect);
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