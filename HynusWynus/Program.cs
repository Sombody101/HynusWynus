using HynusWynus.Classes;
using HynusWynus.Classes.Addons.Modding;
using Spectre.Console;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace HynusWynus;

public class Program
{
    static bool toldYouSo = false;
    public static void Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("This application was designed for Windows 10 and will crash immediately.\r\nPress enter to continue.");
            Console.ReadKey();
            toldYouSo = true;
        }

        // Maximize console window
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "";

#if !DEBUG
        AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
        {
            Logging.LogError("Unhandled exception. Press 'Y' to see exception data or any other key to exit." + (toldYouSo ? " (told ya it would crash)" : ""));
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Write("\r");
                AnsiConsole.WriteException((Exception)e.ExceptionObject);
            }

            Environment.Exit(1);
        };
#endif

        Console.CancelKeyPress += (sender, e) => 
        {
            Console.CursorVisible = true;
        };

        // Init settings
        Paths.Initialize();
        SettingsManager.Initialize();

        // Write logo if it fits
        if (SettingsManager.Settings.ShowLogoOnStart && Console.BufferWidth > logo[0])
            AnsiConsole.Write(new Panel(logo) { Border = BoxBorder.None });
        else
            Logging.LogWarning($"[red]Buffer width is incredibly small ({Console.BufferWidth} chars) : Some if not all functions will be broken[/]");

        if (SettingsManager.Settings.ShowSettingsOnStart)
            SettingsManager.WriteJsonSettings();

        // Import any mods if they exist
        ModImporter.ImportMods();

        AnsiConsole.MarkupLine("\r\nUse '[gold1]help[/]' for a list of commands");

        // enter loop for command input
        while (true)
        {
            var cmd = Logging.ReadInput();
            if (cmd != "")
                CommandParser.Parse(cmd);
        }
    }

    public static readonly string logo =
$"[magenta1]      :::    ::: :::   ::: ::::    ::: :::    :::  ::::::::\r\n" +
      "     :+:    :+: :+:   :+: :+:+:   :+: :+:    :+: :+:    :+:\r\n" +
      "    +:+    +:+  +:+ +:+  :+:+:+  +:+ +:+    +:+ +:+\r\n" +
      "   +#++:++#++   +#++:   +#+ +:+ +#+ +#+    +:+ +#++:++#++\r\n" +
      "  +#+    +#+    +#+    +#+  +#+#+# +#+    +#+        +#+\r\n" +
      " #+#    #+#    #+#    #+#   #+#+# #+#    #+# #+#    #+#\r\n" +
      "###    ###    ###    ###    ####  ########   ########\r\n[/]" +
$"[yellow1]                            :::       ::: :::   ::: ::::    ::: :::    :::  ::::::::\r\n" +
      "                            :+:       :+: :+:   :+: :+:+:   :+: :+:    :+: :+:    :+:\r\n" +
      "                            +:+       +:+  +:+ +:+  :+:+:+  +:+ +:+    +:+ +:+\r\n" +
      "                            +#+  +:+  +#+   +#++:   +#+ +:+ +#+ +#+    +:+ +#++:++#++\r\n" +
      "                            +#+ +#+#+ +#+    +#+    +#+  +#+#+# +#+    +#+        +#+\r\n" +
      "                             #+#+# #+#+#     #+#    #+#   #+#+# #+#    #+# #+#    #+#\r\n" +
      "                              ###   ###      ###    ###    ####  ########   ########[/]\r\n\r\n";
}
