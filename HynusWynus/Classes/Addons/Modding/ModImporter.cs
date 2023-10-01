using HynusWynusModdingAPI;
using HynusWynusModdingAPI.Shared;
using System.Diagnostics;
using System.Reflection;

namespace HynusWynus.Classes.Addons.Modding;

internal static class ModImporter
{
    public static readonly List<AssemblyMod> LoadedMods = new();

    public static void ImportMods()
    {
        // Return as there are no mods to load
        if (!Directory.Exists(Paths.Mods))
            return;

        // Establish API
        API.CommandParser = new()
        {
            GetEnvironmentVariables = ModdingSpecificMethods.GetEnvironmentVars,
            GetEnvironmentVariable = ModdingSpecificMethods.GetEnvironmentVariable,
            GetNullEnvironmentVariable = ModdingSpecificMethods.GetNullEnvironmentVariable,
            SetEnvironmentVariable = ModdingSpecificMethods.SetEnvironmentVariable,

            GetUserDefinedVariables = ModdingSpecificMethods.GetUserDefinedVars,
            GetUserVariable = ModdingSpecificMethods.GetUserVariable,
            GetNullUserVariable = ModdingSpecificMethods.GetNullUserVariable,
            SetUserVariable = ModdingSpecificMethods.SetUserVariable,

            MarkupCommand = CommandParser.MarkupCommand,
            GetCommandHistory = ModdingSpecificMethods.GetCommandHistory,
        };

        API.Logging = new()
        {
            WriteLine = Logging.WriteLine,
            Write = Logging.Write,
            Log = Logging.Log,
            LogWarning = Logging.LogWarning,
            LogError = Logging.LogError,
            WriteTitle = Logging.WriteTitle,
            WriteData = Logging.WriteData,
            WriteException = Logging.WriteException
        };

        var modEntryType = typeof(IMod);
        foreach (var dir in Directory.GetDirectories(Paths.Mods))
            foreach (string file in Directory.GetFiles(dir))
            {
                if (!file.EndsWith(".dll"))
                    continue;

                var modAssembly = Assembly.UnsafeLoadFrom(Path.GetFullPath(file));
                var mod = new AssemblyMod
                {
                    Assembly = modAssembly,
                    Mods = new(),
                    IsLoaded = true
                };

                foreach (Type type in modAssembly.GetTypes().Where(type => modEntryType.IsAssignableFrom(type) && type.IsClass).ToArray())
                {
                    var instance = (IMod)Activator.CreateInstance(type)!;
                    try
                    {
                        var modlet = new Modlet
                        {
                            Mod = instance,
                            Commands = instance.Initialize().GetActiveCommands(),
                        };

                        mod.Mods.Add(modlet);
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteException($"Error importing mod '{instance.Name}'", ex);
                        mod.IsLoaded = false;
                    }
                }

                LoadedMods.Add(mod);
            }
    }
}

internal static class ModFinder
{
    public static string? GetMethodName<T>(Action<T> action)
    {
        if (action == null)
            return null;

        Delegate[] delegates = action.GetInvocationList();
        if (delegates.Length == 0)
            return null;

        MethodInfo methodInfo = delegates[0].Method;

        return methodInfo.Name;
    }

    public static List<Action<HynusWynusModdingAPI.Shared.Command>> FindCommandHandlersFor(string command)
    {
        if (LoadedMods() == 0)
            return new List<Action<HynusWynusModdingAPI.Shared.Command>>();

        string asmName = "";
        string modName = "";
        string modCommand = "";

        var split = command.Split(':');

        if (split.Length > 3)
            return new List<Action<HynusWynusModdingAPI.Shared.Command>>();

        if (split.Length >= 2)
        {
            asmName = split[0];
            modName = split[1];
        }

        if (split.Length == 3)
        {
            modCommand = split[2];
        }
        else if (split.Length == 2)
        {
            modCommand = modName; // Treat the second part as the command name
            modName = asmName;    // Treat the first part as the mod name
            asmName = "";         // Reset the assembly name
        }
        else
            modCommand = split[0];

        var matchingActions = new List<Action<HynusWynusModdingAPI.Shared.Command>>();

        if (asmName != "")
        {
            matchingActions.AddRange(
                ModImporter.LoadedMods
                    .Where(asmMod => asmMod.Assembly.GetName().Name.FixName() == asmName.ToLower())
                    .SelectMany(asmMod => asmMod.Mods
                        .Where(mod => mod.Mod.IndexName.FixName() == modName.FixName() && mod.Commands.Exists(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()))
                        .SelectMany(mod => mod.Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()))
                        .Select(cmd => cmd.CommandHandler)
                    )
            );
        }
        else if (modName != "")
        {
            matchingActions.AddRange(
                ModImporter.LoadedMods
                    .Where(asmMod => asmMod.Mods.Select(mod => mod.Mod.IndexName.FixName()).AsEnumerable().Contains(modName))
                    .SelectMany(asmMod => asmMod.Mods
                        .Where(mod => mod.Mod.IndexName.FixName() == modName.FixName() && mod.Commands.Exists(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()))
                        .SelectMany(mod => mod.Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()))
                        .Select(cmd => cmd.CommandHandler)
                    )
            );
        }
        else
        {
            matchingActions.AddRange(
                ModImporter.LoadedMods
                    .Where(asmMod => asmMod.Mods.Select(mod => mod.Commands).ToArray()[0]
                        .Select(cc => cc.CommandName.FixName()).ToArray()[0].Contains(modCommand.ToLower()))
                    .SelectMany(asmMod => asmMod.Mods
                        .Where(mod => mod.Commands.ToArray()[0].CommandName.ToLower() == modCommand.ToLower())
                        .SelectMany(mod => mod.Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()))
                        .Select(cmd => cmd.CommandHandler)
                    )
            );
        }

        return matchingActions;
    }

    public static List<string> GetCommandHandlerPathsFor(string command)
    {
        if (LoadedMods() == 0)
            return new List<string>();

        string asmName = "";
        string modName = "";
        string modCommand = "";

        var split = command.Split(':');

        if (split.Length > 3)
            return new List<string>();

        if (split.Length >= 2)
        {
            asmName = split[0];
            modName = split[1];
        }

        if (split.Length == 3)
        {
            modCommand = split[2];
        }
        else if (split.Length == 2)
        {
            modCommand = modName; // Treat the second part as the command name
            modName = asmName;    // Treat the first part as the mod name
            asmName = "";         // Reset the assembly name
        }
        else
        {
            modCommand = split[0];
        }

        var matchingPaths = ModImporter.LoadedMods
            .SelectMany(asmMod => asmMod.Mods
                .Where(mod =>
                    (string.IsNullOrEmpty(asmName) || asmMod.Assembly.GetName().Name.FixName() == asmName.ToLower()) &&
                    (string.IsNullOrEmpty(modName) || mod.Mod.IndexName.ToLower() == modName.ToLower()) &&
                    mod.Commands.Exists(cmd => cmd.CommandName.ToLower() == modCommand.ToLower())
                )
                .Select(mod =>
                {
                    string assemblyName = asmMod.Assembly.GetName().Name.FixName();
                    string moduleName = mod.Mod.IndexName;
                    string commandName = mod.Commands.First(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()).CommandName;
                    return $"{assemblyName}:{moduleName}:{commandName}";
                })
            )
            .ToList();

        return matchingPaths;
    }



    public static Action<HynusWynusModdingAPI.Shared.Command>? _FindCommandHandlerFor(string command)
    {
        if (LoadedMods() == 0)
            return null;

        // A ':' means the user is specifying a specific mod. Similar to how mimikatz uses "::" to specify modules, although that's because it's based on
        // how C++ accesses static items, so the inspiration is different
        if (command.Contains(':'))
        {
            // Only if specified by the user (narrows ambiguity)
            string asmName = "";
            string modName = "";
            string modCommand = "";

            {
                var split = command.Split(':');

                // More than three wont point to anything
                if (split.Length > 3)
                    return null;

                if (split.Length == 2)
                    (modName, modCommand) = (split[0], split[1]);
                else if (split.Length == 3)
                    (asmName, modName, modCommand) = (split[0], split[1], split[2]);
            }

            Action<HynusWynusModdingAPI.Shared.Command>? method = null;

            // All of this is just a fluster cluck if LINQ queries to find which object has the command
            // Even I no longer know how to navigate it.
            // Modders beware.
            if (asmName != "")
            {
                var asm = ModImporter.LoadedMods.Where(asmMod => asmMod.Assembly.GetName().Name.FixName() == asmName.ToLower()).ToArray();
                if (asm.Length != 1)
                    return null;

                var mod = asm[0].Mods.Where(mod => mod.Mod.IndexName.ToLower() == modName.ToLower()).ToArray();
                if (mod.Length != 1)
                    return null;

                var cmd = mod[0].Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()).ToArray();
                if (cmd.Length != 1)
                    return null;

                method = cmd[0].CommandHandler;
            }
            else if (modName != "")
            {
                var asm = ModImporter.LoadedMods.Where(asmMod => asmMod.Mods.Select(mod => mod.Mod.IndexName.ToLower()).AsEnumerable().Contains(modName)).ToArray();
                if (asm.Length != 1)
                    return null;

                var mod = asm[0].Mods.Where(mod => mod.Mod.IndexName.ToLower() == modName.ToLower()).ToArray();
                if (mod.Length != 1)
                    return null;

                var cmd = mod[0].Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()).ToArray();
                if (cmd.Length != 1)
                    return null;

                method = cmd[0].CommandHandler;
            }
            else
            {
                var asm = ModImporter.LoadedMods.Where(asmMod => asmMod.Mods.Select(mod => mod.Commands).ToArray()[0].
                    Select(cc => cc.CommandName.ToLower()).ToArray()[0].Contains(modCommand.ToLower())).ToArray();
                if (asm.Length != 1)
                    return null;

                var mod = asm[0].Mods.Where(mod => mod.Commands.ToArray()[0].CommandName.ToLower() == modCommand.ToLower()).ToArray();
                if (mod.Length != 1)
                    return null;

                var cmd = mod[0].Commands.Where(cmd => cmd.CommandName.ToLower() == modCommand.ToLower()).ToArray();
                if (cmd.Length != 1)
                    return null;

                method = cmd[0].CommandHandler;
            }

            return method;
        }

        return null;
    }

    public static int LoadedMods()
        => ModImporter.LoadedMods.Where(asmMod => asmMod.IsLoaded).ToArray().Length;

    public static string FixName(this string? name)
        => name is null ? "N/A" : name.ToLower().Replace(' ', '_');
}

/*
 * Mod tree view
 * 
 * AssemblyMod
 *   - Assembly (Assembly)
 *   - IsLoaded (bool)
 *   - Mods     (List<Modlet>)
 *      - Mod       (IMod)
 *      - Commands      (List<CommandCreator>)
 *          - CommandName           (string)
 *          - CommandDescription    (string)
 *          - CommandHandler        (Action<CommandCreator>)
 */

/// <summary>
/// Each <see cref="System.Reflection.Assembly"/> and the <see cref="IMod"/> instances associated with it
/// </summary>
internal struct AssemblyMod
{
    public AssemblyMod(Assembly asm, List<Modlet> mods)
    {
        Assembly = asm;
        Mods = mods;
    }

    public Assembly Assembly { get; set; }
    public bool IsLoaded { get; set; } = true;
    public List<Modlet> Mods { get; set; }
}

/// <summary>
/// Each <see cref="IMod"/> instance and the command data associated with it
/// </summary>
internal struct Modlet
{
    public Modlet(IMod mod, List<CommandCreator> commands)
    {
        Mod = mod;
        Commands = commands;
    }

    public IMod Mod { get; set; }
    public List<CommandCreator> Commands { get; set; }
}