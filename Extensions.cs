using System.IO;
using System.Text;
using static System.Runtime.InteropServices.Marshal;

namespace SMGWiimoteSoundPlayer
{
    public static class Extensions
    {
        public static short Flip(this short val) =>
            (short)((val & 0xffu) << 8 | (val & 0xff00u) >> 8);

        public static ushort Flip(this ushort val) =>
            (ushort)((val & 0xffu) << 8 | (val & 0xff00u) >> 8);

        public static int Flip(this int val) =>
            (int)((val & 0xffu) << 24 | (val & 0xff00u) << 8 | (val & 0xff0000u) >> 8 | (val & 0xff000000u) >> 24);

        public static uint Flip(this uint val) => 
            (val & 0xffu) << 24 | (val & 0xff00u) << 8 | (val & 0xff0000u) >> 8 | (val & 0xff000000u) >> 24;

        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            var len = SizeOf<T>();
            var ptr = AllocHGlobal(len);
            Copy(reader.ReadBytes(len), 0, ptr, len);
            T buf = PtrToStructure<T>(ptr);
            FreeHGlobal(ptr);
            return buf;
        }

        public static T[] ReadStructs<T>(this BinaryReader reader, int numStructs) where T : struct
        {
            var buf = new T[numStructs];
            var len = SizeOf<T>();
            var ptr = AllocHGlobal(len * numStructs);
            Copy(reader.ReadBytes(len * numStructs), 0, ptr, len * numStructs);
            for (int i = 0; i < numStructs; i++)
                buf[i] = PtrToStructure<T>(ptr + i * len);
            FreeHGlobal(ptr);
            return buf;
        }

        public static string ReadASCII(this BinaryReader input, int size) => Encoding.ASCII.GetString(input.ReadBytes(size), 0, size);

        public static string ReadASCIIZ(this BinaryReader input)
        {
            var start = input.BaseStream.Position;
            var size = 0;

            while (input.BaseStream.ReadByte() - 1 > 0)
                size++;

            input.BaseStream.Position = start;
            var text = input.ReadASCII(size);
            input.BaseStream.Position++;
            return text;
        }
    }
}
