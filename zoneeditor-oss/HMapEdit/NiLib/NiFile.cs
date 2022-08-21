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

		public NiAVObject FindRoot()
		{
			var avObj = ObjectsByRef.Values.OfType<NiAVObject>().Select(obj => obj).FirstOrDefault();

			if (avObj == null)
				return null;

			while (avObj.Parent != null)
				avObj = avObj.Parent;

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

		public void Render(Device device, Effect effect)
		{
			var node = FindRoot() as NiNode;
			if (node != null)
				_Render(device, effect, node, Matrix.Identity);
		}
		private void _Render(Device device, Effect effect, NiAVObject obj, Matrix modelMatrix)
		{
			// TODO filter correctly
			if (obj.Name.Value.ToLower() == "collidee")
				return;

			foreach (var prop in obj.Properties) // TODO other indexes
			{
				if (prop.Object is NiTexturingProperty tex)
				{
					Texture texture = null;
					if (tex.DetailTexture?.Source != null && tex.DetailTexture.Source.IsValid())
					{
						tex.DetailTexture.Source.SetRef(this);
						texture = LocalTextures.Get(tex.DetailTexture.Source.Object.FileName.Value, true, true);
					}
					if (texture == null && tex.BaseTexture?.Source != null && tex.BaseTexture.Source.IsValid())
					{
						tex.BaseTexture.Source.SetRef(this);
						texture = LocalTextures.Get(tex.BaseTexture.Source.Object.FileName.Value, false, true);
					}

					if (texture != null)
						device.SetTexture(0, texture);
				}
			}

			modelMatrix = Matrix.Translation(obj.Translation.X, obj.Translation.Y, obj.Translation.Z) * modelMatrix;
			modelMatrix = Matrix.Scaling(obj.Scale, obj.Scale, obj.Scale) * modelMatrix;
			modelMatrix = obj.Rotation * modelMatrix;

			var mode = _FindDrawMode(obj);
			var oldMode = device.RenderState.CullMode;
			device.RenderState.CullMode = mode == eFaceDrawMode.DRAW_CCW ? Cull.CounterClockwise : (mode == eFaceDrawMode.DRAW_CW ? Cull.Clockwise : Cull.None);
			if (obj is NiTriStrips strips && strips.Data.Object is NiTriStripsData stripsData)
			{
				var vb = new VertexBuffer(typeof(Vector3), stripsData.Vertices.Length, device, Usage.None, VertexFormats.Position, Pool.Managed);
				vb.SetData(stripsData.Vertices, 0, LockFlags.Discard);
				device.SetStreamSource(0, vb, 0);
				if (stripsData.UVSets.Length > 0)
				{
					var ub = new VertexBuffer(typeof(Vector2), stripsData.UVSets[0].Length, device, Usage.None, VertexFormats.Texture0, Pool.Managed);
					ub.SetData(stripsData.UVSets[0], 0, LockFlags.Discard);
					device.SetStreamSource(1, ub, 0);
				}

				var oldWorld = device.Transform.World;
				// device.Transform.World = modelMatrix * device.Transform.World;
				effect.SetValue(EffectHandle.FromString("World"), modelMatrix * device.Transform.World);
				effect.BeginPass(0);
				foreach (var points in stripsData.Points)
				{
					var ib = new IndexBuffer(typeof(ushort), points.Length, device, Usage.None, Pool.Managed);
					ib.SetData(points, 0, LockFlags.Discard);
					device.Indices = ib;

					device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, (int)stripsData.NumVertices, 0, points.Length - 2);
				}
				effect.EndPass();
				// device.Transform.World = oldWorld;
			}
			if (obj is NiTriShape shape && shape.Data.Object is NiTriShapeData shapeData)
			{
				var vb = new VertexBuffer(typeof(Vector3), shapeData.Vertices.Length, device, Usage.None, VertexFormats.Position, Pool.Default);
				vb.SetData(shapeData.Vertices, 0, LockFlags.Discard);
				device.SetStreamSource(0, vb, 0);

				VertexBuffer uvs = null;
				VertexBuffer normals = null;
				if (shapeData.UVSets.Length > 0)
				{
					uvs = new VertexBuffer(typeof(Vector2), shapeData.UVSets[0].Length, device, Usage.WriteOnly, VertexFormats.Texture0, Pool.Default);
					uvs.SetData(shapeData.UVSets[0], 0, LockFlags.Discard);
					device.SetStreamSource(1, uvs, 0);
				}
				if ((shapeData.Normals?.Length ?? 0) > 0)
				{
					normals = new VertexBuffer(typeof(Vector3), shapeData.Normals.Length, device, Usage.None, VertexFormats.Normal, Pool.Default);
					normals.SetData(shapeData.Normals, 0, LockFlags.Discard);
					device.SetStreamSource(2, normals, 0);
				}

				var ib = new IndexBuffer(typeof(ushort), shapeData.Points.Length, device, Usage.None, Pool.Default);
				ib.SetData(shapeData.Points, 0, LockFlags.Discard);
				device.Indices = ib;

				//var oldWorld = device.Transform.World;
				//var oldTex0 = device.Transform.Texture0;
				//device.Transform.World = modelMatrix * device.Transform.World;
				//device.Transform.Texture0 = modelMatrix * device.Transform.Projection;

				effect.SetValue(EffectHandle.FromString("World"), modelMatrix * device.Transform.World);
				effect.BeginPass(0);
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (int)shapeData.NumVertices, 0, shapeData.Triangles.Length);
				effect.EndPass();


				//device.Transform.World = oldWorld;
				//device.Transform.Texture0 = oldTex0;

				vb.Dispose();
				if (uvs != null)
					uvs.Dispose();
				if (normals != null)
					normals.Dispose();
				ib.Dispose();
			}
			device.RenderState.CullMode = oldMode;

			if (obj is NiLODNode lod)
			{
				var near = lod.Children.FirstOrDefault(n => n.IsValid() && n.Object.Name.Value == "near");
				if (near != null)
				{
					_Render(device, effect, near.Object, modelMatrix);
					return;
				}
			}
			if (obj is NiNode node)
				foreach (var child in node.Children)
					if (child.IsValid())
						_Render(device, effect, child.Object, modelMatrix);
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
