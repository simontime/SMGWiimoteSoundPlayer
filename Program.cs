using System;
using System.IO;

namespace SMGWiimoteSoundPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("SpkRes.arc"))
            {
                Console.Error.WriteLine("Error: SpkRes.arc must exist in current directory!");
                Console.Error.WriteLine("Press the any key to continue...");
                Console.ReadKey();
                return;
            }

            var rarc = new RARC("SpkRes.arc");

            Console.WriteLine("Loaded SpkRes.arc...");

            var table = rarc.GetFile("spktable.bct");
            Console.WriteLine("Read spktable.bct from RARC...");
            var wave  = rarc.GetFile("spkwave.csw");
            Console.WriteLine("Read spkwave.csw from RARC...");

            Parsers.PopulateSoundNamesFromBCT(table);
            Console.WriteLine("Read sound names from BCT...");
            Parsers.PopulateSoundsFromCSW(wave);
            Console.WriteLine("Read sounds from CSW...");

            PrintNames:
            Console.WriteLine("\nArguments: 1-{0}, print, exit\n", Parsers.Sounds.Length);

            for (int i = 0; i < Parsers.SoundNames.Length; i++)
                Console.WriteLine("{0}:\t{1}", i + 1, Parsers.SoundNames[i]);

            Console.WriteLine();

            while (true)
            {
                Console.Write("Input: ");

                var soundname = Console.ReadLine();

                if (soundname == "print")
                    goto PrintNames;

                if (soundname == "exit")
                    break;

                try
                {
                    var index = int.Parse(soundname);
                    var audio = Sound.MakeWaveStreamFromPCMData(Parsers.Sounds[index - 1]);
                    Sound.PlayWAV(audio);
                } catch { }
            }
        }
    }
}