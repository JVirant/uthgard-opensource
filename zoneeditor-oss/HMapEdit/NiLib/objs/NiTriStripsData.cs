using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MNL
{
	public class NiTriStripsData : NiTriBasedGeomData
	{
		public bool HasPoints = false;
		public ushort[][] Points;

		public NiTriStripsData(NiFile file, BinaryReader reader)
			: base(file, reader)
		{
			var pointCounts = new ushort[reader.ReadUInt16()];
			for (var i = 0; i < pointCounts.Length; i++)
			{
				pointCounts[i] = reader.ReadUInt16();
			}

			if (Version >= eNifVersion.VER_10_0_1_3)
			{
				HasPoints = reader.ReadBoolean();
			}

			if (Version < eNifVersion.VER_10_0_1_3
				|| HasPoints)
			{
				Points = new ushort[pointCounts.Length][];
				for (var i = 0; i < pointCounts.Length; i++)
				{
					Points[i] = new ushort[pointCounts[i]];
					for (ushort j = 0; j < pointCounts[i]; j++)
					{
						Points[i][j] = reader.ReadUInt16();
					}
				}
			}
		}

		#region DirectX rendering
		private VertexDeclaration _vertexDeclaration = null;
		private VertexBuffer[] _vertices = null;
		private IndexBuffer[] _indices = null;

		public override void DxInit(Device device)
		{
			base.DxInit(device);
			var vertexElements = new List<(VertexElement ve, VertexBuffer vb)>();

			var vb = new VertexBuffer(typeof(Vector3), Vertices.Length, device, Usage.Dynamic, VertexFormats.Position, Pool.Default);
			vb.SetData(Vertices, 0, LockFlags.Discard);
			vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0), vb));

			if (UVSets.Length > 0)
			{
				for (byte i = 0; i < UVSets.Length; i++)
				{
					vb = new VertexBuffer(typeof(Vector2), UVSets[i].Length, device, Usage.Dynamic, (VertexFormats)(i << 8), Pool.Default);
					vb.SetData(UVSets[i], 0, LockFlags.Discard);
					vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, i), vb));
				}
			}
			if ((Normals?.Length ?? 0) > 0)
			{
				vb = new VertexBuffer(typeof(Vector3), Normals.Length, device, Usage.Dynamic, VertexFormats.Normal, Pool.Default);
				vb.SetData(Normals, 0, LockFlags.Discard);
				vertexElements.Add((new VertexElement((short)vertexElements.Count, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0), vb));
			}

			_indices = Points
				.Select(indices =>
				{

					var ib = new IndexBuffer(typeof(ushort), indices.Length, device, Usage.Dynamic, Pool.Default);
					ib.SetData(indices, 0, LockFlags.Discard);
					return ib;
				})
				.ToArray();
			_vertexDeclaration = new VertexDeclaration(device, vertexElements.Select(e => e.ve).Union(new[] { VertexElement.VertexDeclarationEnd }).ToArray());
			_vertices = vertexElements.Select(e => e.vb).ToArray();
		}

		public override void Render(Device device)
		{
			base.Render(device);
			if (_vertices == null)
				throw new Exception("DxInit must be called first");
			for (var i = 0; i < _vertices.Length; i++)
				device.SetStreamSource(i, _vertices[i], 0);

			device.VertexDeclaration = _vertexDeclaration;
			foreach (var indices in _indices)
			{
				device.Indices = indices;
				device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, (int)NumVertices, 0, indices.SizeInBytes / sizeof(ushort) - 2);
			}

			for (var i = 0; i < _vertices.Length; i++)
				device.SetStreamSource(i, null, 0);
		}
		#endregion
	}
}
