namespace HynusWynusModdingAPI;

/// <summary>
/// Exposes specific methods, fields, and properties from the main HynusWynus application for use in a mod
/// </summary>
public static class API
{
    public static CommandParserPointer CommandParser;
    public static LoggingPointer Logging;
    public static SettingsManagerPointer SettingsManager;

    public class CommandParserPointer
    {
        /// <summary>
        /// Get a copy of the variables defined by HynusWynus
        /// <para>
        /// Environment variables cannot be unset by the user, but can be modified
        /// </para>
        /// </summary>
        public d_GetEnvironmentVariables GetEnvironmentVariables;

        /// <summary>
        /// Get an environment variable, or an empty string if it doesn't exist
        /// </summary>
        public d_GetEnvironmentVariable GetEnvironmentVariable;

        /// <summary>
        /// Get an environment variable, or null if it doesn't exist
        /// </summary>
        public d_GetNullEnvironmentVariable GetNullEnvironmentVariable;

        /// <summary>
        /// Set an environment variable
        /// <para>
        /// Environment variables cannot be unset by the user, but can be modified
        /// </para>
        /// </summary>
        public d_SetEnvironmentVariable SetEnvironmentVariable;

        /// <summary>
        /// Get a copy of the variables defined by the user
        /// </summary>
        public d_GetUserDefinedVariables GetUserDefinedVariables;

        /// <summary>
        /// Get a user defined variable, or an empty string if it doesn't exist
        /// </summary>
        public d_GetUserDefinedVariable GetUserVariable;

        /// <summary>
        /// Get a user defined variable, or null if it doesn't exist
        /// </summary>
        public d_GetNullUserDefinedVariable GetNullUserVariable;

        /// <summary>
        /// Set a user defined variable
        /// </summary>
        public d_SetUserDefinedVariable SetUserVariable;

        /// <summary>
        /// Returns a new string with Spectre.Console markup 
        /// </summary>
        public d_MarkupCommand MarkupCommand;

        /// <summary>
        /// All commands entered by the user
        /// </summary>
        public d_GetCommandHistory GetCommandHistory;


        public delegate Dictionary<string, string> d_GetEnvironmentVariables();
        public delegate string d_GetEnvironmentVariable(string variable);
        public delegate string? d_GetNullEnvironmentVariable(string variable);
        public delegate void d_SetEnvironmentVariable(string variable, string value);

        public delegate Dictionary<string, string> d_GetUserDefinedVariables();
        public delegate string d_GetUserDefinedVariable(string variable);
        public delegate string? d_GetNullUserDefinedVariable(string variable);
        public delegate void d_SetUserDefinedVariable(string variable, string value);

        public delegate void d_RegisterCommand(string command, string commandDescription, Dictionary<string, string>? optionsAndDescriptions);
        public delegate string d_MarkupCommand(string commandInput);
        public delegate List<string> d_GetCommandHistory();
    }

    public class LoggingPointer
    {
        /// <summary>
        /// Write a markup line
        /// </summary>
        public d_WriteLine WriteLine;

        /// <summary>
        /// Write markup text (without a newline)
        /// </summary>
        public d_Write Write;

        /// <summary>
        /// Create a log
        /// </summary>
        public d_Log Log;

        /// <summary>
        /// Create a warning log
        /// </summary>
        public d_LogWarning LogWarning;

        /// <summary>
        /// Create a error log
        /// </summary>
        public d_LogError LogError;

        /// <summary>
        /// Write a separator title
        /// </summary>
        public d_WriteTitle WriteTitle;

        /// <summary>
        /// Write data to the console
        /// </summary>
        public d_WriteData WriteData;

        /// <summary>
        /// Write a markup exception with a message
        /// </summary>
        public d_WriteException WriteException;

        public delegate void d_WriteLine(string line = "");
        public delegate void d_Write(string text = "");
        public delegate void d_Log(string message);
        public delegate void d_LogWarning(string log);
        public delegate void d_LogError(string log);
        public delegate void d_WriteTitle(string title);
        public delegate void d_WriteData(string header, params string[] data);
        public delegate void d_WriteException(string message, Exception exception);
    }

    public class SettingsManagerPointer
    {

    }
}