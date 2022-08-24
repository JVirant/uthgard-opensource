using HMapEdit;
using HMapEdit.Tools;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MNL
{
	public class NiFile
	{
		public const uint INVALID_REF = 0xFFFFFFFF;
		public const string CMD_TOP_LEVEL_OBJECT = "Top Level Object";
		public const string CMD_END_OF_FILE = "End Of File";

		public eNifVersion Version
		{
			get { return Header.Version; }
		}

		public readonly string FileName;
		public NiHeader Header;
		public NiFooter Footer;

		public Dictionary<uint, NiObject> ObjectsByRef;

		public NiFile(BinaryReader reader, string path)
		{
			FileName = path;
			Header = new NiHeader(this, reader);
			ReadNiObjects(reader);
			Footer = new NiFooter(this, reader);
			FixRefs();
		}

		private void ReadNiObjects(BinaryReader reader)
		{
			ObjectsByRef = new Dictionary<uint, NiObject>();

			var i = 0;
			while (true)
			{
				var type = "";
				if (Version >= eNifVersion.VER_5_0_0_1)
				{
					if (Version <= eNifVersion.VER_10_1_0_106)
					{
						if (reader.ReadUInt32() != 0)
						{
							throw new Exception("Check value is not zero! Invalid file?");
						}
					}
					type = Header.BlockTypes[Header.BlockTypeIndex[i]].Value;
				}
				else
				{
					var objTypeStrLen = reader.ReadUInt32();
					if (objTypeStrLen > 30 || objTypeStrLen < 6)
					{
						throw new Exception("Invalid object type string length!");
					}
					type = new string(reader.ReadChars((int)objTypeStrLen));

					if (Header.Version < eNifVersion.VER_3_3_0_13)
					{
						if (type == CMD_TOP_LEVEL_OBJECT)
							continue;

						if (type == CMD_END_OF_FILE)
							break;
					}
				}

				uint index;
				if (Version < eNifVersion.VER_3_3_0_13)
				{
					index = reader.ReadUInt32();
				}
				else
				{
					index = (uint)i;
				}

				var csType = Type.GetType("MNL." + type);

				if (csType == null)
				{
					throw new NotImplementedException(type);
				}

				var currentObject = (NiObject)Activator.CreateInstance(csType, new object[] { this, reader });
				ObjectsByRef.Add(index, currentObject);

				if (Version < eNifVersion.VER_3_3_0_13) continue;

				i++;

				if (i >= Header.NumBlocks)
					break;
			}
		}


		private void FixRefs()
		{
			foreach (var obj in ObjectsByRef.Values)
				FixRefs(obj);
		}

		private void FixRefs(object obj)
		{
			foreach (var field in obj.GetType().GetFields())
			{
				if (field.FieldType.Name.Contains("NiRef"))
				{
					if (field.FieldType.IsArray)
					{
						var values = (IEnumerable)field.GetValue(obj);
						if (values == null) continue;
						foreach (dynamic val in values)
						{
							//var method = val.GetType().GetMethod("SetRef");
							//method.Invoke(val, new object[] { this });
							val.SetRef(this);

							if (field.Name == "Children")
							{
								//var isValidMethod = val.GetType().GetMethod("IsValid");
								//if ((bool)isValidMethod.Invoke(val, null))
								if (val.IsValid())
								{
									// var child = val.GetType().GetProperty("Object").GetValue(val) as NiAVObject;
									var child = val.Object as NiAVObject;
									if (child == null)
									{
										throw new Exception("no child");
									}
									child.Parent = (NiNode)obj;
								}
							}
						}
					}
					else
					{
						//var method = field.FieldType.GetMethod("SetRef");
						dynamic value = field.GetValue(obj);
						if (value == null) continue;
						value.SetRef(this);
						//method.Invoke(value, new object[] { this });
					}
				}
			}
		}

		private bool _rootFound = false;
		private NiAVObject _root = null;
		public NiAVObject FindRoot()
		{
			if (_rootFound)
				return _root;
			_rootFound = true;

			var avObj = ObjectsByRef.Values.OfType<NiAVObject>().Select(obj => obj).FirstOrDefault();
			if (avObj == null)
				return null;

			while (avObj.Parent != null)
				avObj = avObj.Parent;

			_root = avObj;
			return avObj;
		}

		private void PrintNifTree()
		{
			var root = FindRoot();
			if (root == null)
			{
				Console.WriteLine("No Root!");
				return;
			}
			var depth = 0;
			PrintNifNode(root, depth);
		}

		private void PrintNifNode(NiAVObject root, int depth)
		{
			var ds = "";

			for (var i = 0; i < depth; i++)
				ds += "*";

			ds += " ";

			Console.WriteLine(ds + " " + root.Name);

			var niNode = root as NiNode;
			if (niNode != null)
			{
				foreach (var child in niNode.Children)
				{
					if (child.IsValid())
						PrintNifNode(child.Object, depth + 1);
				}
			}
		}

		private bool _dxInitDone = false;
		public void DxInit(Device device)
		{
			if (_dxInitDone)
				return;
			var node = FindRoot();
			if (node != null)
				node.DxInit(device);
			_dxInitDone = true;
		}
		public void Render(Device device, Effect effect, Vector3 position)
		{
			var node = FindRoot() as NiNode;
			if (node != null)
				_Render(device, effect, node, Matrix.Identity, ref position);
		}

		private static EffectHandle _handleWorld = EffectHandle.FromString("World");
		private void _Render(Device device, Effect effect, NiAVObject obj, Matrix modelMatrix, ref Vector3 position)
		{
			// TODO filter correctly
			if (obj.Name.Value.ToLower() == "collidee")
				return;

			modelMatrix = Matrix.Translation(obj.Translation.X, obj.Translation.Y, obj.Translation.Z) * modelMatrix;
			modelMatrix = Matrix.Scaling(obj.Scale, obj.Scale, obj.Scale) * modelMatrix;
			modelMatrix = obj.Rotation * modelMatrix;

			if (obj is NiLODNode lod)
			{
				var dist = Vector3.Length(Program.FORM.renderControl1.CAMERA - position) * 0.125;
				if (lod.LODLevels != null)
				{
					for (int i = 0; i < lod.LODLevels.Length; i++)
					{
						var level = lod.LODLevels[i];
						if (lod.Children[i].IsValid() && level.NearExtent <= dist && dist <= level.FarExtent)
						{
							_Render(device, effect, lod.Children[i].Object, modelMatrix, ref position);
							return;
						}
					}
				}
				if (lod.LODLevelData != null && lod.LODLevelData.IsValid() && lod.LODLevelData.Object is NiRangeLODData ranges)
				{
					for (int i = 0; i < ranges.LODLevels.Length; i++)
					{
						var level = ranges.LODLevels[i];
						if (lod.Children[i].IsValid() && level.NearExtent <= dist && dist <= level.FarExtent)
						{
							_Render(device, effect, lod.Children[i].Object, modelMatrix, ref position);
							return;
						}
					}
				}
			}
			if (obj is NiNode node)
				foreach (var child in node.Children)
					if (child.IsValid())
						_Render(device, effect, child.Object, modelMatrix, ref position);
			if (obj is NiTriShape || obj  is NiTriStrips)
			{
				foreach (var prop in obj.Properties)
				{
					if (prop.Object is NiTexturingProperty tex)
					{
						// TODO review this part, it's not ok: UVSetIndex is not the "sampler idx"
						if (tex.BaseTexture?.Source != null && tex.BaseTexture.Source.IsValid())
						{
							tex.BaseTexture.Source.SetRef(this);
							var texture = LocalTextures.Get(tex.BaseTexture.Source.Object.FileName.Value, false, true);
							device.SetTexture((int)tex.BaseTexture.UVSetIndex, texture);
						}
						if (tex.DetailTexture?.Source != null && tex.DetailTexture.Source.IsValid())
						{
							tex.DetailTexture.Source.SetRef(this);
							var texture = LocalTextures.Get(tex.DetailTexture.Source.Object.FileName.Value, true, true);
							device.SetTexture((int)tex.DetailTexture.UVSetIndex, texture);
						}
						if (tex.DarkTexture?.Source != null && tex.DarkTexture.Source.IsValid())
						{
							tex.DarkTexture.Source.SetRef(this);
							var texture = LocalTextures.Get(tex.DarkTexture.Source.Object.FileName.Value, true, true);
							device.SetTexture((int)tex.DarkTexture.UVSetIndex, texture);
						}
					}
				}

				var mode = _FindDrawMode(obj);
				var oldMode = device.RenderState.CullMode;
				device.RenderState.CullMode = mode == eFaceDrawMode.DRAW_CCW ? Cull.CounterClockwise : (mode == eFaceDrawMode.DRAW_CW ? Cull.Clockwise : Cull.None);
				effect.SetValue(_handleWorld, modelMatrix * device.Transform.World);
				effect.BeginPass(0);
				obj.Render(device);
				effect.EndPass();
				device.RenderState.CullMode = oldMode;
			}
		}

		private static eFaceDrawMode _FindDrawMode(NiAVObject obj)
		{
			NiAVObject current = obj;
			while (current != null)
			{
				foreach (var propRef in current.Properties)
				{
					var sprop = propRef.Object as NiStencilProperty;
					if (sprop == null) continue;
					return sprop.FaceDrawMode;
				}

				current = current.Parent;
			}

			return eFaceDrawMode.DRAW_CCW_OR_BOTH;
		}
	}
}
