using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Json;

namespace HynusWynus.Classes;

public class Settings
{
    [JsonProperty("dump_file_output_directory")]
    public string DumpFileOutputDirectory { get; set; } =
    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders",
    "{374DE290-123F-4565-9164-39C4925E467B}",
    "C:\\HYDumps\\")!.ToString()!;

    [JsonProperty("show_exceptions_on_crash")]
    public bool ShowCrashErrors { get; set; } =
#if DEBUG
        true;
#else
        false;
#endif

    //public bool SkipLaunchChecks { get; set; } = false;
    [JsonProperty("show_logo_on_launch")]
    public bool ShowLogoOnStart { get; set; } = true;
    
    [JsonProperty("show_settings_on_launch")]
    public bool ShowSettingsOnStart { get; set; } = false;

    [JsonProperty("bte_lock")]
    public bool BTE_Lock { get; set; } = false;
}

public static class SettingsManager
{
    public static void Initialize()
    {
        if (!File.Exists(Paths.This + "settings.json"))
        {
            SerializedSettings = JObject.Parse(JsonConvert.SerializeObject(Settings));
            return;
        }

        var json = File.ReadAllText(Paths.This + "settings.json");
        var settings = LoadSettingsFromString(json);

        if (settings is null)
        {
            Logging.LogError("Failed to deserialize [yellow]settings.json[/] : Proceeding with default values");
            SerializedSettings = JObject.Parse(JsonConvert.SerializeObject(settings));
            return;
        }

        SerializedSettings = JObject.Parse(json);

        Settings = settings;
    }

    public static Settings Settings { get; set; } = new();
    public static JObject SerializedSettings { get; set; }

    public static void WriteJsonSettings()
    {
        CommandParser.Parse("settings get");
        Console.Write("\r\n");
    }

    public static bool SaveSettingsTo(this Settings settings, string outputFilePath)
    {
        try
        {
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
            return true;
        }
        catch (Exception e)
        {
            Logging.WriteException("Failed to save settings", e);
        }

        return false;
    }

    public static Settings? LoadSettingsFrom(string file)
    {
        try
        {
            return LoadSettingsFromString(File.ReadAllText(file));
        }
        catch (Exception e)
        {
            Logging.WriteException("Failed to save settings", e);
        }

        return null;
    }

    public static Settings? LoadSettingsFromString(string json)
        => JsonConvert.DeserializeObject<Settings>(json);
}