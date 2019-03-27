using System.IO;

namespace SMGWiimoteSoundPlayer
{
    struct BCTHeader
    {
        public int FileCount;
        public int HeaderSize;
        public int StringTableOffset;
        public int Padding;
    }

    struct BCTEntry
    {
        public short FileNumber;
        public byte _Unk1;
        public byte _Unk2;
        public short Param;
        public short Padding;
    }

    struct CSWHeader
    {
        public int DataSize;
        public int FileCount;
    }

    class Parsers
    {
        public static string[] SoundNames;
        public static byte[][] Sounds;

        public static void PopulateSoundNamesFromBCT(byte[] bct)
        {
            using (var strm = new MemoryStream(bct))
            using (var reader = new BinaryReader(strm))
            {
                var hdr = reader.ReadStruct<BCTHeader>();
                var num = hdr.FileCount.Flip();

                strm.Position =
                    hdr.StringTableOffset.Flip() + (num * sizeof(int));

                SoundNames = new string[num];

                for (int i = 0; i < num; i++)
                    SoundNames[i] = reader.ReadASCIIZ();
            }
        }

        public static void PopulateSoundsFromCSW(byte[] csw)
        {
            using (var strm = new MemoryStream(csw))
            using (var reader = new BinaryReader(strm))
            {
                var hdr = reader.ReadStruct<CSWHeader>();
                var num = hdr.FileCount.Flip();

                Sounds = new byte[num][];

                var offsets = new int[num];

                for (int i = 0; i < num; i++)
                    offsets[i] = reader.ReadInt32().Flip();

                for (int i = 0; i < num; i++)
                {
                    strm.Position = offsets[i];
                    Sounds[i] = reader.ReadBytes(reader.ReadInt32().Flip());
                    Sound.EndianSwap16bitPCMData(Sounds[i]);
                }
            }
        }
    }
}
