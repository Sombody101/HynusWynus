using HynusWynusModdingAPI.Shared;

namespace HynusWynusModdingAPI;

public interface IMod
{
    /// <summary>
    /// The name of the <see cref="IMod"/> that is used to resolve ambiguity when referencing commands
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// The name of the <see cref="IMod"/> that is presented to the user
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the <see cref="IMod"/> that will be presented to the user
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The version info for the mod
    /// </summary>
    public ModVersion Version { get; }

    /// <summary>
    /// First method called by HynusWynus, and prior to any user input
    /// </summary>
    /// <returns>
    /// A <see cref="CommandCreatorHost"/> instance used to setup commands for the user to enter
    /// </returns>
    public CommandCreatorHost Initialize();
}