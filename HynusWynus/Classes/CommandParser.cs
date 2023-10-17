using FilePack;
using HynusWynus.Classes;
using HynusWynus.Classes.Addons.Modding;
using Newtonsoft.Json;
using Spectre.Console;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HynusWynusModdingAPI.Shared;

namespace HynusWynus;

public class Command
{
    public string All { get; set; }
    public string[] args { get; set; }
    private string? _cmd;

    public Command(string CommandString)
    {
        // Resolve variable usage
        foreach (var variable in CommandParser.GetVariables(CommandString))
            CommandString = CommandString.Replace(variable,
                CommandParser.EnvironmentVariables.ContainsKey(variable) ? CommandParser.EnvironmentVariables[variable] : "");

        // Remove comments (if any)
        CommandString = CommandParser.ExtractCommand(CommandString);

        var split = CommandParser.ExtractQuotedStrings(CommandParser.SplitIntoStrings(CommandString).Select(str => str.Trim()).ToArray());
        Cmd = split.Length > 0 ? split[0] : "";
        args = split.Length > 1 ? split[1..split.Length] : Array.Empty<string>();

        All = string.Join(" ", split);
    }

    public string Cmd
    {
        get => _cmd is null ? "" : _cmd;
        private set => _cmd = value;
    }

    public string GetArg(int el)
        => --el < args.Length ? args[el] : "";

    public string? GetArgNull(int el)
        => --el < args.Length ? args[el] : null;

    public bool HasArg(string arg)
        => args.Contains(arg);
}

public static partial class CommandParser
{
    internal static readonly List<string> CommandHistory = new();

    /// <summary>
    /// Variables defined by HynusWynus used for volatile configurations or checking what the terminal supports
    /// </summary>
    [JsonProperty("app_defined_variables")]
    public static Dictionary<string, string> EnvironmentVariables { get; set; } = new()
    {
        { "$VERSION", Assembly.GetExecutingAssembly().GetName().Version!.ToString() },
        { "$HOSTNAME", Environment.MachineName },
        { "$HOME", "C:\\Users\\" + Environment.UserName + '\\' },
        { "$USER", Environment.UserName },
        { "$APP", Paths.This },
        { "$TRUECOLOR", AnsiConsole.Profile.Supports(ColorSystem.TrueColor) ? "true" : "false" },
        { "$ENCODING", AnsiConsole.Profile.Encoding.EncodingName },
        { "$PROMPTPREFIX", Environment.MachineName },
        { "$COLORTYPE", AnsiConsole.Profile.Capabilities.ColorSystem.ToString() },
        { "$LEGACYCONSOLE", AnsiConsole.Profile.Capabilities.Legacy ? "true" : "false" },
        { "$SUPPORTSLINKS", AnsiConsole.Profile.Capabilities.Links ? "true" : "false" }
    };

    /// <summary>
    /// Variables defined by the user
    /// </summary>
    [JsonProperty("user_defined_variables")]
    public static Dictionary<string, string> UserDefinedVariables { get; set; } = new();

    internal static readonly List<string> Commands = new()
    {
        //"disable-defender",

        "mimikatz",
        "mimi",
        "katz",

        "peas",

        "procexp",

        "dump",
        "procs",

        "unpack",
        "decrypt",

        "settings",

        "help",
        "exit",
        "vars",
        "history",
        "unset",

        "clear",
        "cls",
        "c",

        "mods",

        // Run commands from other shells
        "pwsh",
        "cmd",
        "echo",

        // Burn The Evidence
        "bte"
    };

    public static void Parse(string command)
    {
        // Splits input into commands separated by a ';', as long as its not in a string
        foreach (var com in GetSeparatedCommands(command))
            ParseCommand(com);
    }

    private static void ParseCommand(string command)
    {
        // Strokes while making commands: 23
        var cmd = new Command(command);

        // Setting a variable
        if (IsVariableAssignment(command))
        {
            var args = cmd.All.Split('=');
            var varName = args[0].Trim();
            string varValue = string.Join("=", args.Skip(1)).Trim();

            // Remove leading and trailing quotes (if present)
            if ((varValue.StartsWith('\"') && varValue.EndsWith('\"')) ||
                (varValue.StartsWith('\'') && varValue.EndsWith('\'')))
                varValue = varValue[1..^1];

            if (EnvironmentVariables.ContainsKey('$' + varName))
                EnvironmentVariables['$' + varName] = varValue;
            else
                UserDefinedVariables['$' + varName] = varValue;

#if DEBUG
            Console.WriteLine("Set $" + varName + " to: \"" + varValue + "\"");
#endif

            return;
        }

        // Handle commands
        switch (cmd.Cmd.ToLower())
        {
            case "clear":
            case "cls":
            case "c":
                Console.Clear();
                break;

            case "mimi":
            case "mimikatz":
            case "katz":
                LaunchMimikatz(cmd);
                break;

            // Launch procexp.exe (when you can't use task manager)
            case "procexp":
                LaunchProcexp();
                break;

            // Launch PEASS (.bat, but working on getting .exe)
            case "peas":
                LaunchPeas(cmd);
                break;

            // Print all processes
            case "procs":
                ShowProcesses(cmd);
                break;

            case "dump":
                DumpApplication(cmd);
                break;

            case "decrypt":
                DecryptApp(cmd);
                break;

            case "decrypt-file":
                break;
            case "encrypt-file":
                break;

            case "exit":
                _ = int.TryParse(cmd.GetArg(1), out int code);
                Environment.Exit(code);
                break;

            case "help":
                DisplayHelp(cmd.GetArg(1));
                break;

            case "vars":
                if (cmd.GetArg(1) is "" or "env")
                    foreach (var variable in EnvironmentVariables)
                        AnsiConsole.MarkupLine(MarkupCommand($"{new string(variable.Key.Skip(1).ToArray())}={variable.Value}"));

                if (cmd.GetArg(1) is "" or "user")
                    foreach (var variable in UserDefinedVariables)
                        AnsiConsole.MarkupLine(MarkupCommand($"{new string(variable.Key.Skip(1).ToArray())}={variable.Value}"));
                break;

            case "history":
                foreach (var str in CommandHistory)
                    AnsiConsole.MarkupLine($"\'{MarkupCommand(str)}`");
                break;

            case "unset":
                foreach (var arg in cmd.args)
                    UserDefinedVariables.Remove('$' + arg);
                break;
            case "unpack":
                UnpackFile(cmd);
                break;

            // Get/Set settings
            case "settings":
                EditSettings(cmd);
                break;

            case "mods":
                ShowMods(cmd);
                break;

            case "pwsh":
                var pwsh = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-c \"{string.Join(" ", cmd.args)}\"",
                    },
                };

                pwsh.Start();
                pwsh.WaitForExit();
                break;

            case "cmd":
                var cmdP = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"{string.Join(" ", cmd.args)}\"",
                    },
                };

                cmdP.Start();
                cmdP.WaitForExit();
                break;

            case "echo":
                Console.WriteLine(string.Join(" ", cmd.args));
                break;

            case "bte":
                if (SettingsManager.Settings.BTE_Lock)
                {
                    Logging.LogError("bte_lock enabled");
                    break;
                }

                Logging.Log("[red]Burn The Evidence[/]");
                Logging.Log("Removing self...");

                Process.Start("cmd.exe", $"/C powershell.exe -c Start-Sleep -Seconds 1; " +
                    $"Remove-Item -Path \"{Paths.This + "Assets"}\" -Recurse; " +
                    $"Remove-Item -Path \"{Paths.This + "HynusWynus.exe"}\"; " +
                    // Remove dll last in case user is running packed version (prevents powershell error from stopping the deletion of other items)
                    $"Remove-Item -Path \"{Paths.This + "HynusWynus.dll"}\"; ");

                Logging.Log("Awaiting exit...");
                Environment.Exit(0);
                break;

            case "":
                return;

            default:
                if (CommandFinish.Finish(cmd.Cmd))
                    break;

                if (!cmd.Cmd.Contains(':'))
                {
                    Logging.LogError($"Unknown command \"[yellow]{cmd.Cmd}[/]\" : Use '[gold1]help[/]' for a list of commands");
                    return;
                }

                var possibleMethods = ModFinder.FindCommandHandlersFor(cmd.Cmd);

                if (possibleMethods.Count == 0)
                {
                    var split = cmd.Cmd.Split(':');

                    Logging.LogError($"Failed to find command '[gold1]{split[^1]}[/]' in {ReplaceLast(MarkupCommand(string.Join(":", split)), split[^1], "")}");
                }
                else if (possibleMethods.Count > 1)
                {
                    Logging.WriteData("[red][[!]][/]\tCommand reference ambiguity among",
                            ModFinder.GetCommandHandlerPathsFor(cmd.Cmd).Select(cmd => '\t' + MarkupCommand(cmd)).ToArray());
                }
                else
                    possibleMethods[0](new HynusWynusModdingAPI.Shared.Command(cmd.All.Split()));
                break;
        }
    }

    /* Switch methods */

    public static void LaunchMimikatz(Command cmd) // Redesign
    {
        if (!File.Exists(Paths.Mimikatz))
        {
            Logging.LogError("Failed to locate mimikatz.exe : Run 'unpack mimikatz -d' ");
            return;
        }

        Logging.Log($"Starting mimikatz...");

        try
        {
            var mimi = new Process()
            {
                StartInfo = new()
                {
                    FileName = Paths.Mimikatz,
                    Arguments = string.Join(" ", cmd.args),
                },
            };

            mimi.Start();
            mimi.WaitForExit();
        }
        catch (Exception ex)
        {
            Logging.WriteException("Error while starting mimikatz.exe", ex);
        }
    }

    public static void LaunchPeas(Command cmd) // Redesign
    {
        Logging.Log($"Starting PEASS...");
        string file;

        switch (cmd.GetArg(1).ToLower())
        {
            case "":
            case "bat":
                file = Paths.PeassBat;
                break;

            case "ps1":
                file = Paths.PeassPs1;
                break;

            case "x64":
                file = Paths.PeassX64;
                break;

            default:
                Logging.LogError($"Unknown peas type '{cmd.GetArg(1)}'");
                return;
        }

        if (!File.Exists(file))
        {
            Logging.LogError($"Failed to locate {Path.GetFileName(file)} : Run 'unpack peas{cmd.GetArg(1).ToLower()} -d' ");
            return;
        }

        try
        {
            var peas = new Process()
            {
                StartInfo = new() { FileName = file, },
            };

            peas.Start();
            peas.WaitForExit();
        }
        catch (Exception ex)
        {
            Logging.WriteException($"Error while starting {Path.GetFileName(file)}", ex);
            Logging.Log("Attempting to launch with PS...");

            Process.Start("powershell.exe", $"-NoProfile -ExecutionPolicy ByPass -File \"{file}\"");
        }
    }

    private static void ShowProcesses(Command cmd)
    {
        string key = "";
        ProcFilter pf = ProcFilter.None;
        bool skipPattern = false;

        switch (cmd.GetArg(1).ToLower())
        {
            case "": // For default search
                break;

            case "-a":
                pf = ProcFilter.ShowAll;
                skipPattern = true;
                break;

            case "-name":
                pf |= ProcFilter.IndexingName;
                break;

            case "-pid":
                pf |= ProcFilter.IndexingPID;
                break;

            case "-si":
                pf |= ProcFilter.IndexingSI;
                break;

            default:
                Logging.LogError($"Unknown filter type '[red]{cmd.GetArg(1)}[/]'");
                return;
        }

        if (cmd.args.Length < 2)
        {
            Logging.LogError("No search value provided");
            return;
        }

        if (!skipPattern)
        {
            key = string.Join(" ", cmd.args.Skip(1));

            if (key.EndsWith('*'))
            {
                pf |= ProcFilter.StartsWith;
                key = key.Remove(key.Length - 1);
            }

            if (key.StartsWith('*'))
            {
                pf |= ProcFilter.EndsWith;
                key = key.Remove(0, 1);
            }

            if (!pf.HasFlag(ProcFilter.StartsWith) && !pf.HasFlag(ProcFilter.EndsWith))
                pf |= ProcFilter.Equals;
        }

        ProcessManager.PrintProcesses(pf, key.Trim());
    }

    public static void EditSettings(Command cmd)
    {
        var setting = cmd.GetArg(2).ToLower();
        switch (cmd.GetArg(1).ToLower())
        {
            case "get":
                if (SettingsManager.SerializedSettings is null)
                {
                    Logging.LogError("Settings have not been configured yet");
                    return;
                }

                if (setting == "")
                    foreach (var set in SettingsManager.SerializedSettings)
                        AnsiConsole.MarkupLine(MarkupCommand($"{set.Key}     \t= {set.Value}"));
                else
                {
                    if (SettingsManager.SerializedSettings.ContainsKey(setting))
                        AnsiConsole.MarkupLine(MarkupCommand($"{setting}={SettingsManager.SerializedSettings[setting]}"));
                    else
                        Logging.LogError($"Failed to find any setting by the name of '{cmd.GetArg(2)}'");
                }
                break;

            case "set":
                if (SettingsManager.SerializedSettings is null)
                {
                    Logging.LogError("Settings have not been configured yet");
                    return;
                }

                if (cmd.args.Length < 2)
                {
                    Logging.LogWarning("No value provided to set");
                    break;
                }

                var value = string.Join(" ", cmd.args[2..]);
                if (SettingsManager.SerializedSettings.ContainsKey(setting))
                {
                    var copy = SettingsManager.SerializedSettings;
                    copy[setting] = value;
                    Settings newSettings;

                    try
                    {
                        newSettings = JsonConvert.DeserializeObject<Settings>(copy.ToString())!;
                    }
                    catch
                    {
                        Logging.LogError($"Invalid value '{value}' for setting '{setting}'");
                        break;
                    }

                    SettingsManager.Settings = newSettings;
                }
                else
                    Logging.LogError($"Failed to find any setting by the name of '{cmd.GetArg(2)}'");
                break;

            case "export":
                if (SettingsManager.Settings.SaveSettingsTo(Paths.This + "settings.json"))
                    Logging.Log($"Settings saved to settings.json");
                break;

            case "":
                Logging.LogError("No action provided");
                break;

            default:
                Logging.LogError($"Unknown action [red]{cmd.GetArg(1)}[/]");
                break;
        }
    }

    public static void ShowMods(Command cmd)
    {
        if (ModImporter.LoadedMods.Count == 0)
        {
            Logging.Log("There are no loaded mods");
            return;
        }

        var moddingRoot = new Tree($"[white]Active mods ({Paths.AppName})[/]").Style("white");

        foreach (var asmMod in ModImporter.LoadedMods)
        {
            var modAssembly = moddingRoot.AddNode($"[purple]{asmMod.Assembly.GetName().Name}[/] {(asmMod.IsLoaded ? "" : "[white]-[/] [red]UNLOADED[/]")}");

            if (cmd.GetArg(1).ToLower() == "-c")
            {
                var commandRoot = asmMod.Assembly.GetName().Name.FixName();
                foreach (var mod in asmMod.Mods)
                {
                    var modNode = modAssembly.AddNode($"[gray]{mod.Mod.IndexName} ([white]{mod.Mod.Version.VersionString}[/])[/]");
                    foreach (var command in mod.Commands)
                        modNode.AddNode(MarkupCommand($"{commandRoot}:{mod.Mod.IndexName.FixName()}:{command.CommandName}"));
                }
            }
            else
                foreach (var mod in asmMod.Mods.Select(mod => mod.Mod))
                    modAssembly.AddNode($"[gray]{mod.IndexName} ([white]{mod.Version.VersionString}[/])[/]");
        }

        AnsiConsole.Write(moddingRoot);
    }

    public static void LaunchProcexp()
    {
        if (File.Exists(Paths.ProcViewer))
        {
            Logging.Log("Launching procexp...");
            Process.Start(Paths.ProcViewer);
        }
        else
            Logging.LogError("Unable to find procexp.exe : Check if it still exists or has been moved");
    }

    public static void DumpApplication(Command cmd)
    {
        if (cmd.GetArg(1) == "")
        {
            Logging.LogError("No argument provided");
            return;
        }

        if (cmd.GetArg(2) == "")
            cmd.All = "index " + cmd.All;

        switch (cmd.GetArg(1).ToLower())
        {
            case "-pid":
                ProcessManager.Dump(Process.GetProcesses().Where(proc => proc.Id.ToString() == cmd.GetArg(2)).ToArray()[0].Id, false);
                return;

            case "-index":

                if (!int.TryParse(cmd.GetArg(2), out int index))
                {
                    Logging.LogError($"Non int value [red]'{cmd.GetArg(2)}'[/] provided");
                    return;
                }

                ProcessManager.Dump(--index);
                return;

            default:
                Logging.LogError($"Unknown specifier '[red]{cmd.GetArg(1)}[/]'");
                return;
        }
    }

    public static void UnpackFile(Command cmd)
    {
        if (cmd.args.Length == 0)
        {
            Logging.LogError("No app(s) provided");
            return;
        }

        bool decrypt = false;
        if (cmd.HasArg("-d"))
        {
            decrypt = true;
            cmd.args = string.Join(" ", cmd.args).Replace("-d", "").Split();
        }

        if (string.Join(" ", cmd.args) == "")
        {
            Logging.LogError("No app(s) provided");
            return;
        }

        if (cmd.HasArg("all"))
            cmd.args = "peasps1 peasbat peasx64 mimikatz procexp".Split();

        foreach (var arg in cmd.args.Select(str => str.Trim()))
        {
            string outputDir;
            byte[] app;

            switch (new string(arg.Reverse().ToArray()).ToLower())
            {
                case "1spsaep": // Peas.ps1
                    outputDir = Paths.DirUnpackedPeassPs1;
                    app = Resources._1spsaep;
                    break;

                case "tabsaep": // Peas.bat
                    outputDir = Paths.DirUnpackedPeassBat;
                    app = Resources.tabsaep;
                    break;

                case "46xsaep": // PeasX64.exe
                    outputDir = Paths.DirUnpackedPeassX64;
                    app = Resources._46xsaep;
                    break;

                case "ztakimim": // mimikatz.exe
                    outputDir = Paths.DirUnpackedMimikatz;
                    app = Resources.ztakimim;
                    break;

                case "pxecorp":
                    outputDir = Paths.ProcViewer.Replace("procexp.exe", "");
                    app = Resources.procexp;
                    break;

                case "":
                    continue;

                default:
                    Logging.LogError($"Unknown app '{arg}'");
                    continue;
            }

            try
            {
                if (Directory.Exists(outputDir))
                {
                    Logging.Log($"Clearing old {arg} files...");
                    Directory.Delete(outputDir, true);
                }
                else if (File.Exists(outputDir))
                {
                    Logging.Log($"Clearing old {arg} files...");
                    File.Delete(outputDir);
                }

                string tmpZip = Paths.Assets + "tmpzip.zip";
                File.WriteAllBytes(tmpZip, app);

                ZipFile.ExtractToDirectory(tmpZip, outputDir);

                Logging.Log($"{arg} extracted to {outputDir}");
                File.Delete(tmpZip);

            }
            catch (Exception ex)
            {
                Logging.WriteException($"Failed to unzip {arg}", ex);
                return;
            }
        }

        if (decrypt)
        {
            Logging.Log($"Decrypting...");
            cmd.args = string.Join(" ", cmd.args).Replace("procexp", "").Split();
            DecryptApp(cmd);
        }
    }

    public static void DecryptApp(Command cmd)
    {
        if (cmd.args.Length == 0)
        {
            Logging.LogError("No app(s) provided");
            return;
        }

        if (cmd.HasArg("all"))
            cmd.args = "peasps1 peasbat peasx64 mimikatz".Split();

        foreach (var arg in cmd.args)
        {
            string outputApp;
            string encPath;

            switch (new string(arg.Reverse().ToArray()).ToLower())
            {
                case "1spsaep": // Peas.ps1
                    outputApp = Paths.PeassPs1;
                    encPath = Paths.DirUnpackedPeassPs1;
                    break;

                case "tabsaep": // Peas.bat
                    outputApp = Paths.PeassBat;
                    encPath = Paths.DirUnpackedPeassBat;
                    break;

                case "46xsaep": // PeasX64.exe
                    outputApp = Paths.PeassX64;
                    encPath = Paths.DirUnpackedPeassX64;
                    break;

                case "imim":
                case "ztak":
                case "ztakimim": // mimikatz.exe
                    outputApp = Paths.Mimikatz;
                    encPath = Paths.DirUnpackedMimikatz;
                    break;

                case "":
                    continue;

                default:
                    Logging.LogError($"Unknown app '{arg}'");
                    continue;
            }

            try
            {
                if (File.Exists(outputApp))
                {
                    Logging.Log($"Clearing old {arg} files...");
                    File.Delete(outputApp);
                }

                Paths.CreateIfNot(Path.GetDirectoryName(outputApp)!);

                if (Directory.Exists(encPath))
                    Decrypt(encPath, outputApp, arg);
                else
                    Logging.LogError($"{arg} has not been unpacked yet");

                Logging.Log($"decrypted {arg}");
            }
            catch (Exception e)
            {
                Logging.WriteException("Failed to decrypt", e);
            }
        }
    }

    /* Class methods */

    public static void Decrypt(string EncPath, string AppPath, string FailName)
    {
        if (Directory.Exists(EncPath))
        {
            try
            {
                FilePacker.DecryptFile(EncPath, AppPath, true);
            }
            catch (Exception ex)
            {
                Logging.WriteException($"\r\nFailed to decrypt {FailName} files", ex);
                File.Delete(AppPath);
            }
        }
        else
            Logging.LogError($"Could not find encrypted files. Run 'unpack {FailName}' to install them.");
    }

    public static void AltFile(bool IsEnc, string FileName, string OutputFile, int PartitionSize)
    {
        if (IsEnc)
            FilePacker.EncryptFile(FileName, PartitionSize, OutputFile);
        FilePacker.DecryptFile(FileName, OutputFile);
    }

    public static void DisplayHelp(string option = "")
    {
        static string AssembleDescription(string command, string description, Dictionary<string, string>? options = null)
        {
            StringBuilder output = new();

            output.Append($"\t[gold1]{command}[/][white]:".PadMarkupRight(35));
            output.Append(description + "[/]\r\n");

            if (options is not null)
                foreach (var element in options)
                {
                    output.Append($"\t\t[red]{element.Key}[/][white]:".PadMarkupRight(35));
                    output.Append(element.Value + "[/]\r\n");
                }

            return output.ToString();
        }

        if (option != "" && !Commands.Contains(option))
        {
            Logging.LogError($"Unknown command '[cyan]{option}[/]'");
            return;
        }

        if (option is "")
            AnsiConsole.MarkupLine("    [white]Find more Windows 10 privilege escalation tricks at[/] [cyan]https://book.hacktricks.xyz/windows-hardening/windows-local-privilege-escalation[/]\r\n");

        if (option is "" or "c" or "cls" or "clear")
            AnsiConsole.MarkupLine(AssembleDescription(
                "clear", "Clear the screen",
                new() { { "aliases", "c, cls" } }
            ));

        if (option is "" or "mimi" or "katz" or "mimikatz")
            AnsiConsole.MarkupLine(AssembleDescription(
                "mimikatz", "Launches mimikatz.exe (used to access data inside certain dump files)",
                new()
                {
                    { "aliases", "mimi, katz" },

                    // Mimikatz commands (too lazy to make it work, so just lots of backspaces)
                    { "[cyan]Mimikatz commands[/]", $"[red]Theses commands will only work inside mimikatz (Not {Paths.AppName})[/]" },
                    { "\texit",         "Quit mimikatz" },
                    { "\tcls",          "Clear screen (doesn't work with redirections, like PsExec)" },
                    { "\tsleep",        "Sleep an amount of milliseconds" },
                    { "\tlog",          "Log mimikatz input/output to file" },
                    { "\tbase64",       "Switch file input/output base64" },
                    { "\tversion",      "Display some version information" },
                    { "\tcd",           "Change or display current directory" },
                    { "\tlocaltime",    "Displays system local date and time (OJ command)" },
                    { "\thostname",     "Displays system local hostname" }
                }
            ));

        if (option is "" or "settings")
        {
            AnsiConsole.MarkupLine(AssembleDescription(
                "settings", $"Gets or sets configurations for {Paths.AppName} (uses JSON name value, see them below)",
                new()
                {
                    { "get <setting>", "Presents the setting value" },
                    { "set <setting> <value>", "Sets the setting value"},
                }
            ));

            AnsiConsole.MarkupLine(AssembleDescription(
                "Settings options", "",
                new()
                {
                    { "dump_file_output_directory", "The directory that dump files are placed (Defaults to Downloads, or 'C:\\' when unavailable)" },
                    { "show_exceptions_on_crash", "Write C# exceptions when they are thrown (Defaults to false)" },
                    { "show_logo_on_launch", $"Enables the {Paths.AppName} logo to be shown when launched (Defaults to true)" },
                    { "show_settings_on_launch", "Write settings.json to the screen on launch (only available when there is a settings.json file available)" }
                }
            ));
        }

        if (option is "" or "procs")
            AnsiConsole.MarkupLine(AssembleDescription(
                "procs <option> [[(*)<term>(*)]]", $"Shows running processes (Defaults to apps with the same session ID as {Paths.AppName})",
                new()
                {
                    {"-name", "Filters processes based on their name" },
                    {"-pid", "Filters processes based on their PID (process ID)" },
                    {"-si", "Filters processes based on their SI (session ID)" },
                    {"(*)<term>(*)", "A string of text, or number, with a wildcard on either side for search filtering\r\n\t\t\t(Example: [gold1]procs[/] [red]-name[/] *nus*" },
                }
            ));

        if (option is "" or "bte")
            AnsiConsole.MarkupLine(AssembleDescription(
                "bte", $"[red]Burn The Evidence[/] | Removes all files assosiated with {Paths.AppName}. Including Assets/*, and HynusWynus.exe/.dll"

                ));

    }

    /* String command manipulation */
    
    public static string MarkupCommand(string commandStr)
    {
        commandStr = commandStr.EscapeMarkup();
        var noMarkupCmdsString = commandStr;
        var noMarkupCmds = commandStr.Trim().Split();

        foreach (var word in noMarkupCmds)
        {
            if (word.Length > 1 && word[0] == '-')
                commandStr = ReplaceFirst(commandStr, word, $"[red]{word}[/]");

            if (UInt128.TryParse(word.Replace('*', ' '), out var _))
                commandStr = ReplaceFirst(commandStr, word, $"[yellow]{word}[/]");
        }

        // A colon means the user is using a full reference path
        if (noMarkupCmds[0].Contains(':'))
        {
            var command = "";
            var split = noMarkupCmds[0].Split(':');

            if (split.Length == 2)
                command += $"[lime]{split[0]}[/]:[gold1]{split[1]}[/]";
            else if (split.Length == 3)
                command += $"[purple]{split[0]}[/]:[lime]{split[1]}[/]:[gold1]{split[2]}[/]";

            commandStr = ReplaceFirst(commandStr, noMarkupCmds[0], command);
        }
        else
            foreach (string com in Commands)
            {
                if (noMarkupCmds[0].ToLower() == com)
                {
                    commandStr = ReplaceFirst(commandStr, noMarkupCmds[0], $"[gold1]{noMarkupCmds[0]}[/]");
                    break;
                }
            }

        // Variable assignment
        if (IsVariableAssignment(commandStr))
        {
            var newSplit = commandStr.Trim().Split('=');
            var value = string.Join("", newSplit[1..]);
            commandStr = ReplaceFirst(commandStr, newSplit[0], $"[orange1]{newSplit[0]}[/]");
            commandStr = ReplaceFirst(commandStr, '=' + value, $"=[yellow]{value}[/]");
        }

        // comment
        var comment = ExtractComment(commandStr);
        commandStr = ReplaceLast(commandStr, comment, $"[grey]{RemoveMarkupInitiators(comment)}[/]");

        // Variable usage
        foreach (var variable in GetVariables(noMarkupCmdsString))
            commandStr = commandStr.Replace(variable, $"[orange1]{variable}[/]");

        // Color quoted strings [(") and (')]
        foreach (var str in GetQuoteSeparated(noMarkupCmdsString))
            commandStr = ReplaceFirst(commandStr, str, $"[darkorange3_1]{str}[/]");

        foreach (var str in GetByteValueSeparated(noMarkupCmdsString))
            commandStr = ReplaceFirst(commandStr, str, $"[olive]{str}[/]");

        return commandStr;
    }

    public static string ReplaceFirst(string str, string term, string replace)
    {
        int position = str.IndexOf(term);
        return position < 0 ? str : string.Concat(str.AsSpan(0, position), replace, str.AsSpan(position + term.Length));
    }

    public static string ReplaceLast(string str, string term, string replace)
    {
        int position = str.LastIndexOf(term);
        return position < 0 ? str : string.Concat(str.AsSpan(0, position), replace, str.AsSpan(position + term.Length));
    }

    public static string[] ExtractQuotedStrings(string[] input)
    {
        List<string> output = new();

        foreach (string item in input)
        {
            if ((item.StartsWith('\"') && item.EndsWith('\"')) ||
                (item.StartsWith('\'') && item.EndsWith('\'')))
                output.Add(item.Trim('\'', '"'));
            else
                output.Add(item);
        }

        return output.ToArray();
    }

    public static List<string> SplitIntoStrings(string input)
    {
        var parts = new List<string>();
        var currentPart = new StringBuilder();
        bool inQuotation = false;
        char quotationType = '\0';

        foreach (char c in input)
        {
            if (c is '"' or '\'' or '`')
            {
                if (!inQuotation)
                {
                    inQuotation = true;
                    quotationType = c;
                }
                else if (c == quotationType)
                {
                    inQuotation = false;
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
                else
                    currentPart.Append(c);
            }
            else if (c == ' ' && !inQuotation)
            {
                if (currentPart.Length > 0)
                {
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
            }
            else
                currentPart.Append(c);
        }

        if (currentPart.Length > 0)
            parts.Add(currentPart.ToString());

        return parts;
    }

    /* Regex */

    public static List<string> GetVariables(string input)
        => VariableRegex().Matches(input).Select(match => match.Value).ToList();

    public static List<string> GetQuoteSeparated(string input)
        => QuotedValueRegex().Matches(input).Select(match => match.Value).ToList();

    public static List<string> GetQuoteSeparatedWithSemicolon(string input)
        => CommandSeparatorWithSemicolonRegex().Matches(input).Select(match => match.Value).ToList();

    public static List<string> GetByteValueSeparated(string input)
    => ByteValueRegex().Matches(input).Select(match => match.Value).ToList();

    public static List<string> GetSeparatedCommands(string input)
        => CommandSeparatorRegex().Matches(input).Select(match => match.Value.Trim()).ToList();

    public static string RemoveMarkupInitiators(string input)
        => MarkupInitiatorRegex().Replace(input, "");

    public static string ExtractCommand(string command)
    {
        var match = CommentRegex().Match(command);
        return match.Success ? command[..match.Groups["comment"].Index].TrimEnd() : command.Trim();
    }

    public static string ExtractComment(string input)
    {
        var match = CommentRegex().Match(input);
        return match.Success ? match.Groups["comment"].Value.Trim() : "";
    }

    public static int GetCommentIndex(string command)
    {
        var match = CommentRegex().Match(command);
        return match.Success ? match.Groups["comment"].Index : -1;
    }

    public static bool IsVariableAssignment(string input)
        => VariableAssignmentRegex().IsMatch(input);

    /* Compile time regex */

    [GeneratedRegex("\\$[A-Za-z0-9_\\-]+")]
    private static partial Regex VariableRegex();

    [GeneratedRegex("[A-Za-z0-9_\\-]+\\s*=")]
    private static partial Regex VariableAssignmentRegex();

    [GeneratedRegex(@"(?<!\\)\""[^\""]*\""|(?<!\\)'[^']*'")]
    private static partial Regex QuotedValueRegex();

    [GeneratedRegex("`[^`]*`")]
    private static partial Regex ByteValueRegex();

    [GeneratedRegex(@"(?:[^""';]*(?:""[^""]*""|'[^']*')[^""';]*)+|[^""';]+")]
    private static partial Regex CommandSeparatorRegex();

    [GeneratedRegex(@"(?:[^""';]*(?:""[^""]*""|'[^']*')[^""';]*)+(?=;|$)")]
    private static partial Regex CommandSeparatorWithSemicolonRegex();

    [GeneratedRegex(@"(?<comment>(?://|#)[^\n]*)")]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"\[[^\[\]]*\](?!\])")]
    private static partial Regex MarkupInitiatorRegex();
}