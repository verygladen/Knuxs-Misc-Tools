﻿namespace Knuxs_Misc_Tools.WrathOfCortex.HGO_Chunk
{
    // TODO: What the hell is this? Some sort of matrix table? What's the extra data afterwards?
    public class INST
    {
        public List<INSTEntry1> UnknownDataStruct_1 { get; set; } = new();

        public List<INSTEntry2> UnknownDataStruct_2 { get; set; } = new();

        public void Read(BinaryReaderEx reader)
        {
            // Basic Chunk Header.
            string chunkType = reader.ReadNullPaddedString(4);
            uint chunkSize = reader.ReadUInt32();

            uint matrixCount = reader.ReadUInt32(); // Not sure what this data is, but it looks to have a Matrix as part of it.

            for (int i = 0; i < matrixCount; i++)
            {
                INSTEntry1 entry = new();

                entry.UnknownMatrix4x4_1 = reader.Read4x4Matrix();
                entry.MatrixIndex = reader.ReadUInt32(); // TODO: Verify.
                entry.UnknownUInt32_1 = reader.ReadUInt32();
                entry.UnknownUInt32_2 = reader.ReadUInt32();
                reader.JumpAhead(0x4); // Always 0.

                UnknownDataStruct_1.Add(entry);
            }

            uint UnknownCount = reader.ReadUInt32(); // Count of whatever this is.
            for (int i = 0; i < UnknownCount; i++)
            {
                INSTEntry2 entry = new();

                reader.JumpAhead(0x40); // Literally always 0? Not sure what the hell this is about.
                entry.UnknownFloat_1 = reader.ReadSingle();
                entry.UnknownFloat_2 = reader.ReadSingle();
                entry.UnknownFloat_3 = reader.ReadSingle();
                reader.JumpAhead(0x4); // Always 1.
                entry.UnknownFloat_4 = reader.ReadSingle();
                reader.JumpAhead(0x8); // Always 0.
                entry.UnknownFloat_5 = reader.ReadSingle();

                UnknownDataStruct_2.Add(entry);
            }

            // Align to 0x4.
            reader.FixPadding();
        }

        public void Write(BinaryWriterEx writer)
        {
            // Chunk Identifier.
            writer.Write("TSNI");

            // Save the position we'll need to write the chunk's size to and add a dummy value in its place.
            long chunkSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write how many of the first entry type there is in this file.
            writer.Write(UnknownDataStruct_1.Count);

            // Write all the first entry types.
            for (int i = 0; i < UnknownDataStruct_1.Count; i++)
            {
                writer.Write(UnknownDataStruct_1[i].UnknownMatrix4x4_1);
                writer.Write(UnknownDataStruct_1[i].MatrixIndex);
                writer.Write(UnknownDataStruct_1[i].UnknownUInt32_1);
                writer.Write(UnknownDataStruct_1[i].UnknownUInt32_2);
                writer.WriteNulls(0x4);
            }

            // Write how many of the second entry type there is in this file.
            writer.Write(UnknownDataStruct_2.Count);

            // Write all the second entry types.
            for (int i = 0; i < UnknownDataStruct_2.Count; i++)
            {
                writer.WriteNulls(0x40);
                writer.Write(UnknownDataStruct_2[i].UnknownFloat_1);
                writer.Write(UnknownDataStruct_2[i].UnknownFloat_2);
                writer.Write(UnknownDataStruct_2[i].UnknownFloat_3);
                writer.Write(0x1);
                writer.Write(UnknownDataStruct_2[i].UnknownFloat_4);
                writer.WriteNulls(0x8);
                writer.Write(UnknownDataStruct_2[i].UnknownFloat_5);
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

    public class INSTEntry1
    {
        public Matrix4x4 UnknownMatrix4x4_1 { get; set; }

        public uint MatrixIndex { get; set; }

        public uint UnknownUInt32_1 { get; set; }

        public uint UnknownUInt32_2 { get; set; }
    }

    public class INSTEntry2
    {
        public float UnknownFloat_1 { get; set; }

        public float UnknownFloat_2 { get; set; }

        public float UnknownFloat_3 { get; set; }

        public float UnknownFloat_4 { get; set; }

        public float UnknownFloat_5 { get; set; }
    }
}
