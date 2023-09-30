using FilePack;
using HynusWynus.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HynusWynus;

// Try to include peas stuff https://github.com/carlospolop/PEASS-ng/tree/master/winPEAS/winPEASbat
public static class Paths
{
    public const string AppName = "[magenta1]Hynus[/][yellow1]Wynus[/]";

    public static void Initialize() 
    {
        // Directory checks
        CreateIfNot(Assets);
        CreateIfNot(Misc);
        CreateIfNot(Payloads);
    }

    public static readonly string This = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string Assets = This + "Assets\\";
    public static readonly string Mods = Assets + "mods\\";

    public static readonly string Payloads = Assets + "payloads\\";
    public static readonly string DirUnpacked = Payloads + "unpacked\\";

    public static readonly string DirMimikatz = Payloads + "mimikatz\\";
    public static readonly string DirUnpackedMimikatz = DirUnpacked + "mimikatz\\";
    public static readonly string DirUnpackedMimilib = DirUnpacked + "mimilib\\";
    public static readonly string Mimikatz = DirMimikatz + "mimikatz.exe";
    public static readonly string MimiLib = DirMimikatz + "mimilib.dll";

    public static readonly string DirPeass = Payloads + "peass\\";
    public static readonly string DirUnpackedPeassBat = DirUnpacked + "bat-peass\\";
    public static readonly string DirUnpackedPeassPs1 = DirUnpacked + "ps1-peass\\";
    public static readonly string DirUnpackedPeassX64 = DirUnpacked + "x64-peass\\";
    public static readonly string PeassBat = DirPeass + "peass.bat";
    public static readonly string PeassPs1 = DirPeass + "peass.ps1";
    public static readonly string PeassX64 = DirPeass + "peassx64.exe";

    public static readonly string Misc = Assets + "misc\\";
    public static readonly string ProcViewer = Misc + "procexp.exe";

    public static void CreateIfNot(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}