using System;
using System.Collections.Generic;
using System.IO;
using HMapEdit.Tools;
using Microsoft.DirectX.Direct3D;

namespace HMapEdit
{
	/// <summary>
	/// Local (zone) Textures
	/// </summary>
	public class LocalTextures
	{
		public readonly Device Device;
		private readonly Texture _objsolid;
		private readonly Dictionary<string, Texture> m_Textures = new Dictionary<string, Texture>();

		public LocalTextures(Device device, Texture objsolid)
		{
			Device = device;
			_objsolid = objsolid;
		}

		public void Set(string file, Texture texture)
		{
			if (m_Textures.ContainsKey(file))
				return;
			m_Textures.Add(file, texture);
		}

		/// <summary>
		/// Retrieves a Texture
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public Texture Get(string file, bool noAlt, bool searchEverywhere = false)
		{
			if (file == "empty")
				return null; //simply.. nothing
			if (string.IsNullOrEmpty(file))
				return noAlt ? null : _objsolid;

			string n = Path.GetFileName(file).ToLower();

			//Otherwise: Try to load
			if (!m_Textures.ContainsKey(n))
			{
				string f = "";
				if (GameData.Exists(file))
					f = file;

				if (GameData.Exists(f))
				{
					Console.WriteLine("Loading Texture " + f);
					Texture t;
					if (file.Contains("patch"))
						t = TextureLoader.FromStream(Device, GameData.Open(f), 0, 0, 1, Usage.None, Format.A8R8G8B8, Pool.Managed, Filter.Box, Filter.Box, 0);
					else
						t = TextureLoader.FromStream(Device, GameData.Open(f));
					m_Textures.Add(n, t);
					return m_Textures[n]; //loaded
				}

				if (searchEverywhere)
				{
					var (stream, path) = GameData.FindTex(file);
					if (stream != null)
					{
						m_Textures.Add(n, TextureLoader.FromStream(Device, stream));
						return m_Textures[n]; //loaded
					}
					if (file.ToLower().EndsWith(".tga") || file.ToLower().EndsWith(".nif")) // there is one texture ending with nif (a bug ?!)
					{
						var (stream2, _) = GameData.FindTex(file.Substring(0, file.Length - 3) + "dds");
						if (stream2 != null)
						{
							m_Textures.Add(n, TextureLoader.FromStream(Device, stream2));
							return m_Textures[n]; //loaded
						}
					}
					if (noAlt)
						return null;
				}

				m_Textures.Add(n, _objsolid);
				Console.WriteLine("Missing Texture: " + file);
			}

			return m_Textures[n]; //loaded
		}

		/// <summary>
		/// Get by Texture
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public string Get(Texture t)
		{
			foreach (KeyValuePair<string, Texture> kv in m_Textures)
			{
				if (kv.Value == t)
					return kv.Key;
			}

			return null;
		}

		/// <summary>
		/// Clears all textures
		/// </summary>
		public void Clear()
		{
			m_Textures.Clear();
		}
	}
}