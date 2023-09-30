using HynusWynusModdingAPI;
using HynusWynusModdingAPI.Shared;

namespace HW_TEST;

public class TestEntryPoint : IMod
{
    public string IndexName { get; set; } = "hw_test";
    public string Name { get; set; } = "Modding Test For HynusWynus";
    public string Description { get; set; } = "(description) Test project for HW";
    public ModVersion Version { get; set; } = new("69.69.69");

    public CommandCreatorHost Initialize() 
    {
        API.Logging.Log("HW Mod Test connected : Initializing");

        return new CommandCreatorHost(new List<CommandCreator>() { 
            new CommandCreator("command", "", new Action<Command>(CommandHandler)),
        });
    }

    public static void CommandHandler(Command cmd)
    {
        API.Logging.Log("Command reached");
    }
}

public class SecondaryTestEntryPoint : IMod
{
    public string IndexName { get; set; } = "1hw_test";
    public string Name { get; set; } = "2 Modding Test For HynusWynus";
    public string Description { get; set; } = "(description) Test project for HW 2";
    public ModVersion Version { get; set; } = new();

    public CommandCreatorHost Initialize()
    {
        API.Logging.Log("HW Mod Test connected : Initializing");

        return new CommandCreatorHost(new List<CommandCreator>() {
            new CommandCreator("command", "", new Action<Command>(CommandHandler)),
        });
    }

    public static void CommandHandler(Command cmd)
    {
        API.Logging.Log("Command reached");
    }
}