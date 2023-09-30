namespace HynusWynusModdingAPI.Shared;

public class Command
{
    public string[] args = Array.Empty<string>();
    private string? _cmd;

    public Command(string[] commandStringArray)
    {
        if (commandStringArray.Length == 0)
            return;

        _cmd = commandStringArray[0];

        if (commandStringArray.Length > 1)
            args = commandStringArray[1..];
    }

    /// <summary>
    /// Get only the command, without arguments, from the command string
    /// </summary>
    public string Cmd
    {
        get => _cmd is null ? "" : _cmd;
        private set => _cmd = value;
    }

    /// <summary>
    /// Gets whole command string (No setter to prevent word splitting)
    /// </summary>
    public string All
    {
        get => _cmd + string.Join(" ", args);
    }

    /// <summary>
    /// Get an argument from a specific index, or an empty string if the index doesn't exist
    /// </summary>
    /// <param name="el"></param>
    /// <returns></returns>
    public string GetArg(int el)
        => --el < args.Length ? args[el] : "";

    /// <summary>
    /// Get an argument from a specific index, or null if the index doens't exist
    /// </summary>
    /// <param name="el"></param>
    /// <returns></returns>
    public string? GetArgNull(int el)
        => --el < args.Length ? args[el] : null;

    /// <summary>
    /// Checks if args[] has a specific string element (Essentially a shorthand for string[].Contains())
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public bool HasArg(string arg)
        => args.Contains(arg);
}

/*
public class Command
{
    public string All = "";
    public string[] args;
    private string? _cmd;

    public Command(string CommandString)
    {
        // Resolve variable usage
        foreach (var variable in API.CommandParser.GetVariables(CommandString))
            CommandString = CommandString.Replace(variable,
                API.CommandParser.EnvironmentVariables.ContainsKey(variable) ? API.CommandParser.EnvironmentVariables[variable] : "");

        // Remove comments (if any)
        CommandString = API.CommandParser.ExtractCommand(CommandString);

        var split = API.CommandParser.ExtractQuotedStrings(API.CommandParser.SplitIntoStrings(CommandString).Select(str => str.Trim()).ToArray());
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

public class Command
{
    /// <summary>
    /// Get the entire command string
    /// </summary>
    public d_All All;

    /// <summary>
    /// Get all arguments for command, excluding the command itself
    /// </summary>
    public d_args Args;

    /// <summary>
    /// Get the command without any of the arguments
    /// </summary>
    public d_Cmd Cmd;

    /// <summary>
    /// Get the argument at a specific index, or an empty string if the index cannot be found
    /// </summary>
    public d_GetArg GetArgAt;

    /// <summary>
    /// Get the argument at a specific index, or null if the index cannot be found
    /// </summary>
    public d_GetArgNull GetArgNullAt;

    /// <summary>
    /// Checks if argument list contains a specific string
    /// </summary>
    public d_HasArg CmdHasArg;

    public delegate string d_All();
    public delegate string[] d_args();
    public delegate string d_Cmd();
    public delegate string d_GetArg(int el);
    public delegate string? d_GetArgNull(int el);
    public delegate bool d_HasArg(string arg);
}
*/