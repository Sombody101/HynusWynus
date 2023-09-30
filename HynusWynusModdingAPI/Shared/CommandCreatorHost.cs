using HynusWynusModdingAPI.Shared.Exceptions;

namespace HynusWynusModdingAPI.Shared;

public class CommandCreatorHost
{
    /// <summary>
    /// Holds the <see cref="CommandCreator"/> instances associated with your mod and is passed to HynusWynus via <see cref="IMod.Initialize"/>
    /// </summary>
    /// <param name="addedCommands"></param>
    public CommandCreatorHost(List<CommandCreator> addedCommands)
        => this.addedCommands = addedCommands;

    /// <summary>
    /// Adds a <see cref="CommandCreator"/> for use in HynusWynus
    /// </summary>
    /// <param name="commandCreator"></param>
    public void AddCommand(CommandCreator commandCreator)
        => addedCommands.Add(commandCreator);

    /// <summary>
    /// 
    /// </summary>
    /// <returns>
    /// a <see cref="List{T}"/> of all added <see cref="CommandCreator"/> instances
    /// </returns>
    public List<CommandCreator> GetActiveCommands()
        => new(addedCommands);

    private readonly List<CommandCreator> addedCommands = new();
}

/// <summary>
/// Assists with the creation of commands
/// </summary>
public class CommandCreator
{
    private string commandName = "";

    /// <summary>
    /// Creates a new instance of <see cref="CommandCreator"/>, but with all default values (Best for programmatically assigning values)
    /// </summary>
    public CommandCreator() { }

    /// <summary>
    /// Creates a new instance of <see cref="CommandCreator"/> and initializes all values, ready for use with <see cref="CommandCreatorHost"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="commandDescription"></param>
    /// <param name="commandHandler"></param>
    public CommandCreator(string command, string commandDescription, Action<Command> commandHandler)
    {
        CommandName = command;
        CommandDescription = commandDescription;
        CommandHandler = commandHandler;
    }

    /// <summary>
    /// The name of the command the user will enter (This cannot be empty)
    /// </summary>
    public string CommandName
    {
        get => commandName;
        set
        {
            if (value == "")
                throw new InvalidCommandData("The command name cannot be empty");

            if (value == ":")
                throw new InvalidCommandData("Commands cannot contain ':', it's used for command ambiguity");

            commandName = value;
        }
    }

    /// <summary>
    /// The description to be presented to the user when they use the help command (This can be empty, but isn't suggested)
    /// </summary>
    public string CommandDescription { get; set; } = "The developer forgot to add a description!";

    /// <summary>
    /// The method which will be invoked when the user enters the command matching CommandName
    /// </summary>
    public Action<Command> CommandHandler { get; set; }
}