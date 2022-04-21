﻿using FraGag.Compression;
using Marathon.IO;

namespace Knuxs_Misc_Tools.Storybook
{
    public class FileEntry
    {
        public string? FileName { get; set; }

        public byte[]? Data { get; set; }

        public uint UnknownUInt32_1 { get; set; }
    }
    internal class ONE
    {
        public List<FileEntry> Files = new();

        /// <summary>
        /// Loads a Sonic and the Secret Rings or Sonic and the Black Knight ONE Archive.
        /// </summary>
        /// <param name="filepath">The archive to load.</param>
        /// <param name="decompress">Whether to decompress the binary data when reading.</param>
        /// <param name="extractionPath">Path to extract the data to.</param>
        public void Load(string filepath, bool decompress = true, string? extractionPath = null)
        {
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            uint fileCount = reader.ReadUInt32();
            reader.JumpAhead(0xC); // Always 0x10 then an offset to the first file's binary data then either four nulls or four FF bytes.

            for (int i = 0; i < fileCount; i++)
            {
                string fileName = reader.ReadNullPaddedString(0x20);
                int fileIndex = reader.ReadInt32();
                uint fileOffset = reader.ReadUInt32();
                int fileLength = reader.ReadInt32();
                uint UnknownUInt32_3 = reader.ReadUInt32();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's location.
                reader.JumpTo(fileOffset);

                // Read the specified amount of bytes.
                byte[] binary = reader.ReadBytes(fileLength);

                // Save this file.
                FileEntry file = new()
                {
                    FileName = fileName,
                    Data = binary,
                    UnknownUInt32_1 = UnknownUInt32_3
                };

                // Decompress this file.
                if (decompress)
                    file.Data = Prs.Decompress(file.Data);

                Files.Add(file);

                // Jump back for the next one.
                reader.JumpTo(position);
            }

            reader.Close();

            // If we have a path specified, then extract to it.
            if (extractionPath != null)
                Extract(extractionPath);
        }
        public void Save(string filepath, bool compress = true)
        {
            // Set up the writer.
            BinaryWriterEx writer = new(File.OpenWrite(filepath), true);

            // Header.
            writer.Write(Files.Count);
            writer.Write(0x10);
            writer.AddOffset("BinaryStart");
            writer.WriteNulls(4); // TODO: Do the versions where these bytes are FF FF FF FF mean anything?

            // Recompress the files.
            // Concerningly, the recompression does not match the game's original files. Is that a problem?
            if (compress)
                for (int i = 0; i < Files.Count; i++)
                    Files[i].Data = Prs.Compress(Files[i].Data);

            // Write the file offset table.
            for (int i = 0; i < Files.Count; i++)
            {
                writer.WriteNullPaddedString(Files[i].FileName, 0x20);
                writer.Write(i);
                writer.AddOffset($"File{i}");
                writer.Write(Files[i].Data.Length);
                writer.Write(Files[i].UnknownUInt32_1); // TODO: What does this mean?
            }

            // Write the file binary data.
            writer.FillOffset("BinaryStart");
            for (int i = 0; i < Files.Count; i++)
            {
                writer.FillOffset($"File{i}");
                writer.Write(Files[i].Data);
            }

            writer.Close();
        }

        /// <summary>
        /// Extracts the files to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            Directory.CreateDirectory(directory);

            foreach (FileEntry file in Files)
                File.WriteAllBytes($@"{directory}\{file.FileName}", file.Data);
        }
    }
}
