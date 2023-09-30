using FilePack;
using System.IO.Compression;

namespace PayloadPrep;

internal class Program
{
    // Single bit shit and split into 32KB files (super duper hacker stuff)
    //FilePacker.EncryptFile(inFile, 32_000, outFileDir);
    //FilePacker.DecryptFile(inFile, 32_000, outFileDir);

    static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
    static readonly string Resources = AppPath + "Resources";
    static readonly string Obfuscated = AppPath + "Obfuscated";
    static readonly string Output = AppPath + "..\\..\\..\\..\\HynusWynus\\Compressed";

    // shit doesnt even work
    //static readonly string Obfuscator = AppPath + "..\\..\\..\\Invoke-Stealth.ps1";

    static void Main()
    {
        if (Directory.Exists(Output))
            Directory.Delete(Output, true);

        if (Directory.Exists(Obfuscated))
            Directory.Delete(Obfuscated, true);
        //Directory.CreateDirectory(Obfuscated);

        foreach (var file in Directory.GetFiles(Resources))
        {
            var filename = Path.GetFileName(file);
            Console.WriteLine($"Working on " + filename);

            string writeFile = file;
            // PS1 obfuscation
            //if (Path.GetExtension(file).ToLower() is ".ps1")
            //{
            //    Console.WriteLine("Obfuscating " + filename);
            //
            //    writeFile = Obfuscated + "\\" + filename;
            //    File.Copy(file, writeFile);
            //    Process.Start("powershell", $"{Obfuscator} {writeFile} -technique all");
            //}

            string outputPath = Output + "\\" + filename + "_comp";
            FilePacker.EncryptFile(writeFile, 32_000, outputPath);

            ZipFile.CreateFromDirectory(outputPath, Output + "\\" + new string(filename.Reverse().ToArray()) + ".zip");
        }
    }
}