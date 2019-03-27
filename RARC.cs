using System.IO;
using System.Runtime.InteropServices;

namespace SMGWiimoteSoundPlayer
{
    [StructLayout(LayoutKind.Sequential)]
    struct RARCHeader
    {
        public uint    Magic;
        public uint    Length;
        public uint    HeaderLength;
        public uint    DataOffset;
        public uint    DataLength;
        public uint    DataLength2;
        public ulong  _Padding1;
        public uint    NodeCount;
        public uint    FirstNodeOffset;
        public uint    DirCount;
        public uint    FirstDirOffset;
        public uint    StringTableLength;
        public uint    StringTableOffset;
        public ushort  FileCount;
        public ushort _Padding2;
        public uint   _Padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RARCNode
    {
        public uint   Magic;
        public int    StringOffset;
        public ushort StringHash;
        public ushort FileEntryCount;
        public int    FileEntryOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RARCFileEntry
    {
        public ushort Index;
        public ushort StringHash;
        public ushort Type;
        public ushort StringOffset;
        public int    FileOffset;
        public int    FileLength;
        public uint  _Padding;
    }

    class RARC
    {
        BinaryReader File;
        RARCHeader Header;

        public RARC(string filename)
        {
            File   = new BinaryReader(System.IO.File.OpenRead(filename));
            Header = File.ReadStruct<RARCHeader>();

            Header.DataOffset        = Header.DataOffset.Flip();
            Header.FirstDirOffset    = Header.FirstDirOffset.Flip();
            Header.StringTableOffset = Header.StringTableOffset.Flip();
        }

        public RARCNode GetNode(int i)
        {
            File.BaseStream.Position = 
                Marshal.SizeOf<RARCHeader>() + i * Marshal.SizeOf<RARCNode>();

            var node = File.ReadStruct<RARCNode>();

            node.StringOffset    = node.StringOffset.Flip();
            node.StringHash      = node.StringHash.Flip();
            node.FileEntryCount  = node.FileEntryCount.Flip();
            node.FileEntryOffset = node.FileEntryOffset.Flip();

            return node;
        }

        public RARCFileEntry GetFileEntry(int i)
        {
            File.BaseStream.Position =
                Header.FirstDirOffset + i * Marshal.SizeOf<RARCFileEntry>() + 0x20;
   
            var entry = File.ReadStruct<RARCFileEntry>();

            entry.Index        = entry.Index.Flip();
            entry.StringOffset = entry.StringOffset.Flip();
            entry.FileOffset   = entry.FileOffset.Flip();
            entry.FileLength   = entry.FileLength.Flip();

            return entry;
        }

        public byte[] GetFile(string name, RARCNode node = default)
        {
            node = node.Magic == default ? GetNode(0) : node;

            for (int i = 0; i < node.FileEntryCount; ++i)
            {
                var entry = GetFileEntry(node.FileEntryOffset + i);

                if (entry.Index == 0xFFFF)
                {
                    if (entry.StringOffset != 0 && entry.StringOffset != 2)
                        GetFile(name, GetNode(entry.FileOffset));
                }
                else
                {
                    File.BaseStream.Position =
                        entry.StringOffset + Header.StringTableOffset + 0x20;

                    if (File.ReadASCIIZ() == name)
                    {
                        File.BaseStream.Position =
                            entry.FileOffset + Header.DataOffset + 0x20;
                        return File.ReadBytes(entry.FileLength);
                    }
                }
            }

            return null;
        }
    }
}