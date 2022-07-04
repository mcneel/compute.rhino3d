using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.RemoteCompute.RhinoCompute;

namespace RhinoCompute_Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"C:\Users\alombardi\GitHub\RemoteComputeClient_Prototypes\SampleScripts\SampleRemoteScripts";
            List<string> ghFiles = Directory.GetFiles(folder).Where(f => f.EndsWith(".gh")).ToList();

            if (!ghFiles.Any())
            {
                Console.WriteLine("No file found.");
                return;
            }

            Console.WriteLine("\nFiles found:");
            for (int i = 1; i <= ghFiles.Count(); i++)
                Console.WriteLine($"\t({i})\t{ghFiles[i - 1].Replace(folder, "")}");

            Console.Write("\nEnter file number to be executed, or 'c' to close:\n");
            string fileNumberRead = Console.ReadLine();
            int fileNumber = -1;
            if (!int.TryParse(fileNumberRead, out fileNumber) && fileNumberRead != "c")
            {
                Console.WriteLine("Incorrect input.");
                return;
            }

            if (fileNumberRead == "c")
                return;

            string definitionPath = ghFiles[fileNumber - 1];

            var archive = definitionPath.GHArchiveFromFilepath();
            var document = archive.GHDocument();
        }
    }
}
