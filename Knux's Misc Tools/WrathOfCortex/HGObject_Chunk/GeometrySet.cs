﻿namespace Knuxs_Misc_Tools.WrathOfCortex.HGObject_Chunk
{
    public class GeometrySet
    {
        public List<Geometry> Read(BinaryReaderEx reader)
        {
            List<Geometry> Geometry = new();

            // Basic Chunk Header.
            string chunkType = reader.ReadNullPaddedString(4);
            uint chunkSize = reader.ReadUInt32();

            // Count of how many Geometry entries are in this chunk.
            uint geometryCount = reader.ReadUInt32();

            // Loop through based on count.
            for (int i = 0; i < geometryCount; i++)
            {
                // Create a new Geometry entry.
                Geometry geometry = new();

                reader.JumpAhead(0x4); // Always 1.
                geometry.Type = reader.ReadUInt32();

                if (geometry.Type == 0)
                {
                    reader.JumpAhead(0xC); // Always 0.
                    uint MeshCount = reader.ReadUInt32();

                    for (int m = 0; m < MeshCount; m++)
                    {
                        Mesh mesh = new();
                        mesh.MaterialIndex = reader.ReadUInt32();
                        uint VertexCount = reader.ReadUInt32();

                        // Read the specified amount of vertices.
                        for (int v = 0; v < VertexCount; v++)
                        {
                            Vertex vertex = new()
                            {
                                Position = reader.ReadVector3(),
                                Normals = reader.ReadVector3(),
                                Colours = reader.ReadBytes(4),
                                TextureCoordinates = reader.ReadVector2()
                            };
                            mesh.Vertices.Add(vertex);
                        }

                        reader.JumpAhead(0x8); // Always a 0 then a 1.

                        mesh.Primitive = new();
                        mesh.Primitive.Type = reader.ReadUInt32();
                        uint faceTableShortCount = reader.ReadUInt32();

                        // If the primitive type is 6, then this is a triangle strip.
                        if (mesh.Primitive.Type == 6)
                        {
                            // Track how many shorts we've read.
                            uint readShorts = 0;

                            // Keep going until we've read the right amount.
                            while (readShorts < faceTableShortCount)
                            {
                                // Find out how many values are in this triangle strip.
                                ushort triCount = reader.ReadUInt16();

                                // Increment readShorts as these count numbers are part of it.
                                readShorts++;

                                // Set up a list to store the values.
                                List<ushort> triangleStrip = new();

                                // Loop through based on the amount of tris in this strip.
                                for (int s = 0; s < triCount; s++)
                                {
                                    triangleStrip.Add(reader.ReadUInt16());
                                    readShorts++;
                                }

                                // Add this triangle strip to the list of triangle strips in this primitive.
                                mesh.Primitive.TriangleStrips.Add(triangleStrip);
                            }
                        }

                        // If the primitive type is 5, then this should be a triangle list.
                        // TODO: This produces incorrect faces when exporting, how are these lists made?
                        else
                        {
                            for (int f = 0; f < faceTableShortCount; f++)
                                mesh.Primitive.FaceIndices.Add(reader.ReadUInt16());
                        }

                        reader.JumpAhead(0x8); // Always 0.

                        geometry.Meshes.Add(mesh);
                    }

                    Geometry.Add(geometry);
                }
                else
                {
                    // Seems to be data for flat planes, Position is the position in the world and texture coordinates is the scale.
                    uint MeshCount = reader.ReadUInt32();
                    for (int m = 0; m < MeshCount; m++)
                    {
                        Mesh mesh = new();
                        mesh.MaterialIndex = reader.ReadUInt32();
                        uint VertexCount = reader.ReadUInt32();
                        mesh.UnknownUInt32_1 = reader.ReadUInt32(); // Seems to be either 0 or 1?
                        reader.JumpAhead(0x4); // Always 0.

                        for (int v = 0; v < VertexCount; v++)
                        {
                            // TODO: Is this right? I seriously have doubts about this one especially.
                            Vertex vertex = new()
                            {
                                Position = reader.ReadVector3(),
                                TextureCoordinates = reader.ReadVector2(),
                                Colours = reader.ReadBytes(4)
                            };
                            mesh.Vertices.Add(vertex);
                        }

                        geometry.Meshes.Add(mesh);
                    }

                    Geometry.Add(geometry);
                }
            }

            // Align to 0x4.
            reader.FixPadding();

            return Geometry;
        }

        public void Write(BinaryWriterEx writer, List<Geometry> geometry)
        {
            // Chunk Identifier.
            writer.Write("0TSG");

            // Save the position we'll need to write the chunk's size to and add a dummy value in its place.
            long chunkSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write the amount of geometry sets in this file.
            writer.Write(geometry.Count);

            // Write the geometry sets
            for (int i = 0; i < geometry.Count; i++)
            {
                // Value here is always 1.
                writer.Write(0x1);

                // Write this geometry set's type.
                writer.Write(geometry[i].Type);

                // Write the vertex and face data depending on the type.
                if (geometry[i].Type == 0)
                {
                    // This set of bytes is always 0.
                    writer.WriteNulls(0xC);

                    // Write the amount of meshes in this geometry set.
                    writer.Write(geometry[i].Meshes.Count);

                    // Write each mesh's data.
                    for (int m = 0; m < geometry[i].Meshes.Count; m++)
                    {
                        // Write this mesh's material index.
                        writer.Write(geometry[i].Meshes[m].MaterialIndex);

                        // Write the amount of vertices this mesh has.
                        writer.Write(geometry[i].Meshes[m].Vertices.Count);

                        // Write each vertex in this mesh.
                        foreach (Vertex? vertex in geometry[i].Meshes[m].Vertices)
                        {
                            writer.Write(vertex.Position);
                            writer.Write(vertex.Normals);
                            writer.Write(vertex.Colours);
                            writer.Write(vertex.TextureCoordinates);
                        }

                        // This data is always 00 00 00 00 00 00 00 01
                        writer.Write(0x0);
                        writer.Write(0x1);

                        // Write this mesh's primitive type.
                        writer.Write(geometry[i].Meshes[m].Primitive.Type);

                        // Write the data depending on the Primitive Type.
                        if (geometry[i].Meshes[m].Primitive.Type == 6)
                        {
                            // Placeholder size writing.
                            long triStripSizePos = writer.BaseStream.Position;
                            uint size = 0;
                            writer.Write("SIZE");

                            // Loop through each triangle strip
                            foreach (List<ushort>? triangleStrip in geometry[i].Meshes[m].Primitive.TriangleStrips)
                            {
                                // Write the amount of faces in this triangle strip.
                                writer.Write((ushort)triangleStrip.Count);
                                size++;

                                // Write each face in this triangle strip.
                                foreach (ushort face in triangleStrip)
                                {
                                    writer.Write(face);
                                    size++;
                                }
                            }

                            // Save our position.
                            long triStripEndPos = writer.BaseStream.Position;

                            // Jump to write the calculated size then jump back.
                            writer.BaseStream.Position = triStripSizePos;
                            writer.Write(size);
                            writer.BaseStream.Position = triStripEndPos;
                        }
                        else
                        {
                            // Write the face count.
                            writer.Write(geometry[i].Meshes[m].Primitive.FaceIndices.Count);

                            // Write the face indices.
                            foreach (var face in geometry[i].Meshes[m].Primitive.FaceIndices)
                                writer.Write(face);
                        }

                        // This set of bytes is always 0.
                        writer.WriteNulls(0x8);
                    }
                }
                else
                {
                    // Write the amount of meshes in this geometry set.
                    writer.Write(geometry[i].Meshes.Count);

                    // Write each mesh's data.
                    for (int m = 0; m < geometry[i].Meshes.Count; m++)
                    {
                        // Write this mesh's material index.
                        writer.Write(geometry[i].Meshes[m].MaterialIndex);

                        // Write the amount of vertices this mesh has.
                        writer.Write(geometry[i].Meshes[m].Vertices.Count);

                        // Write the unknown value these types have.
                        writer.Write((uint)geometry[i].Meshes[m].UnknownUInt32_1);

                        // Value here is always 0.
                        writer.WriteNulls(0x4);

                        // Write each vertex in this mesh.
                        foreach (Vertex? vertex in geometry[i].Meshes[m].Vertices)
                        {
                            writer.Write(vertex.Position);
                            writer.Write(vertex.TextureCoordinates);
                            writer.Write(vertex.Colours);
                        }
                    }
                }
            }

            // Align to 0x4.
            writer.FixPadding(0x4);

            // Calculate the chunk size.
            uint chunkSize = (uint)(writer.BaseStream.Position - (chunkSizePos - 0x4));

            // Save our current position.
            long pos = writer.BaseStream.Position;

            // Fill in the chunk size.
            writer.BaseStream.Position = chunkSizePos;
            writer.Write(chunkSize);

            // Jump to our saved position so we can continue.
            writer.BaseStream.Position = pos;
        }
    }

    public class Geometry
    {
        public uint Type { get; set; }

        public List<Mesh> Meshes { get; set; } = new();
    }

    public class Mesh
    {
        public uint MaterialIndex { get; set; }

        public uint? UnknownUInt32_1 { get; set; }

        public List<Vertex> Vertices { get; set; } = new();

        public Primitive Primitive { get; set; }
    }

    public class Vertex
    {
        public Vector3 Position { get; set; }

        public Vector3 Normals { get; set; }

        public byte[] Colours { get; set; }

        public Vector2 TextureCoordinates { get; set; }
    }

    public class Primitive
    {
        public uint Type { get; set; } // Either 5 or 6? 5 is standard faces, 6 is a triangle strip of some sort.

        public List<List<ushort>> TriangleStrips { get; set; } = new();

        public List<ushort> FaceIndices { get; set; } = new();
    }
}
