using System.IO;
using System.Media;

namespace SMGWiimoteSoundPlayer
{
    class Sound
    {
        public static void EndianSwap16bitPCMData(byte[] sound)
        {
            for (int i = 0; i < sound.Length; i += sizeof(short))
            {
                sound[i] ^= sound[i + 1];
                sound[i + 1] ^= sound[i];
                sound[i] ^= sound[i + 1];
            }
        }

        public static Stream MakeWaveStreamFromPCMData(byte[] sound)
        {
            var strm = new MemoryStream();
            var writer = new BinaryWriter(strm);

            writer.Write("RIFF".ToCharArray());
            writer.Write(sound.Length + 28);
            writer.Write("WAVEfmt ".ToCharArray());

            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);

            writer.Write(6000);
            writer.Write(12000);

            writer.Write((short)2);
            writer.Write((short)16);

            writer.Write("data".ToCharArray());
            writer.Write(sound.Length);

            writer.Write(sound);

            strm.Position = 0;

            return strm;
        }

        public static void PlayWAV(Stream sound)
        {
            using (var player = new SoundPlayer(sound))
                player.PlaySync();
        }
    }
}
