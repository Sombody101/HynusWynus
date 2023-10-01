namespace HynusWynusModdingAPI.Shared;

public class Command
{
    public string[] args { get; set; } = Array.Empty<string>();
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