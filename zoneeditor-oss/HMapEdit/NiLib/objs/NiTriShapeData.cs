using System.IO;

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
    }
}
