using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MNL
{
	public class NiTriShapeData : NiTriBasedGeomData
	{
		public uint NumTrianglePoints;
		public ushort[] Points;
		public bool HasTriangles;
		public Triangle[] Triangles;
		public ushort[][] MatchGroups;

		public NiTriShapeData(NiFile file, BinaryReader reader)
			: base(file, reader)
		{
			NumTrianglePoints = reader.ReadUInt32();

			if (Version >= eNifVersion.VER_10_1_0_0)
				HasTriangles = reader.ReadBoolean();

			if (Version <= eNifVersion.VER_10_0_1_2
				|| (HasTriangles || Version >= eNifVersion.VER_10_0_1_3))
			{
				Triangles = new Triangle[NumTriangles];
				Points = new ushort[NumTriangles * 3];
				for (var i = 0; i < NumTriangles; i++)
				{
					Triangles[i] = new Triangle(reader);
					Points[i * 3 + 0] = Triangles[i].X;
					Points[i * 3 + 1] = Triangles[i].Y;
					Points[i * 3 + 2] = Triangles[i].Z;
				}
			}

			if (Version >= eNifVersion.VER_3_1)
			{
				var numMatchGroups = reader.ReadUInt16();
				MatchGroups = new ushort[numMatchGroups][];
				for (var i = 0; i < numMatchGroups; i++)
				{
					var numVertices = reader.ReadUInt16();
					MatchGroups[i] = new ushort[numVertices];
					for (var c = 0; c < numVertices; c++)
					{
						MatchGroups[i][c] = reader.ReadUInt16();
					}
				}
			}
		}

		#region DirectX rendering
		private VertexDeclaration _vertexDeclaration = null;
		private VertexBuffer[] _vertices = null;
		private IndexBuffer _indices = null;

		public override void DxInit(Device device)
		{
			base.DxInit(device);
			var vertexElements = new List<(VertexElement ve, VertexBuffer vb)>();

			var vb = new VertexBuffer(typeof(Vector3), Vertices.Length, device, Usage.None, VertexFormats.Position, Pool.Managed);
			vb.SetData(Vertices, 0, LockFlags.Discard);
			vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0), vb));

			if (UVSets.Length > 0)
			{
				for (byte i = 0; i < UVSets.Length; i++)
				{
					vb = new VertexBuffer(typeof(Vector2), UVSets[i].Length, device, Usage.None, (VertexFormats)(i << 8), Pool.Managed);
					vb.SetData(UVSets[i], 0, LockFlags.Discard);
					vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, i), vb));
				}
			}
			if ((Normals?.Length ?? 0) > 0)
			{
				vb = new VertexBuffer(typeof(Vector3), Normals.Length, device, Usage.None, VertexFormats.Normal, Pool.Managed);
				vb.SetData(Normals, 0, LockFlags.Discard);
				vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0), vb));
			}

			_indices = new IndexBuffer(typeof(ushort), Points.Length, device, Usage.None, Pool.Managed);
			_indices.SetData(Points, 0, LockFlags.Discard);
			_vertexDeclaration = new VertexDeclaration(device, vertexElements.Select(e => e.ve).Union(new [] { VertexElement.VertexDeclarationEnd }).ToArray());
			_vertices = vertexElements.Select(e => e.vb).ToArray();
		}

		public override void Render(Device device)
		{
			base.Render(device);
			if (_vertices == null)
				throw new Exception("DxInit must be called first");

			for (var i = 0; i < _vertices.Length; i++)
				device.SetStreamSource(i, _vertices[i], 0);
			device.Indices = _indices;

			device.VertexDeclaration = _vertexDeclaration;
			device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (int)NumVertices, 0, NumTriangles);

			for (var i = 0; i < _vertices.Length; i++)
				device.SetStreamSource(i, null, 0);
		}
		#endregion
	}
}
