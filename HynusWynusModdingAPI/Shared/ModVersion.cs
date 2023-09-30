using HynusWynusModdingAPI.Shared.Exceptions;
using System.Reflection;

namespace HynusWynusModdingAPI.Shared;

public readonly struct ModVersion
{
    /// <summary>
    /// Creates a new <see cref="ModVersion"/> with the version info of the mods <see cref="Assembly"/>
    /// </summary>
    public ModVersion()
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        if (assemblyVersion is null)
        {
            AsString = "N/A";
            return;
        }

        Major = (uint)assemblyVersion.Major;
        Minor = (uint)assemblyVersion.Minor;
        Build = (uint)assemblyVersion.Build;
        Revision = (uint)assemblyVersion.Revision;
        AsString = assemblyVersion.ToString();
    }

    public ModVersion(uint major, uint minor, uint build, uint revision)
    {
        Major = major;
        Minor = minor;
        Build = build;
        Revision = revision;
        AsString = $"{major}.{minor}.{build}.{revision}";
    }

    public ModVersion(string version)
    {
        var split = version.Split('.');

        if (split.Length != 4)
            throw new InvalidVersionStringFormat("The version string must have 4 segments Major.Minor.Build.Revision");

        if (!uint.TryParse(split[0], out var major))
            throw new InvalidVersionStringFormat("The version string Major must be a valid UInt32");
        if (!uint.TryParse(split[1], out var minor))
            throw new InvalidVersionStringFormat("The version string Minor must be a valid UInt32");
        if (!uint.TryParse(split[2], out var build))
            throw new InvalidVersionStringFormat("The version string Build must be a valid UInt32");
        if (!uint.TryParse(split[3], out var revision))
            throw new InvalidVersionStringFormat("The version string Revision must be a valid UInt32");

        Major = (uint)major;
        Minor = (uint)minor;
        Build = (uint)build;
        Revision = (uint)revision;
        AsString = version;
    }

    public readonly uint Major { get; } = 0;
    public readonly uint Minor { get; } = 0;
    public readonly uint Build { get; } = 0;
    public readonly uint Revision { get; } = 0;

    /// <summary>
    /// Returns the format string in the format "{Major}.{Minor}.{Build}.{Revision}"
    /// </summary>
    public readonly string AsString { get; }

    /// <summary>
    /// Returns the version number in the string format "v{Major}.{Minor}.{Build}.{Revision}"
    /// </summary>
    public readonly string VersionString { get => AsString == "N/A" ? "N/A" : 'v' + AsString; }
}