namespace HynusWynusModdingAPI;

/// <summary>
/// Exposes specific methods, fields, and properties from the main HynusWynus application for use in a mod
/// </summary>
public static class API
{
    public static CommandParserPointer CommandParser { get; set; }
    public static LoggingPointer Logging { get; set; }

    public class CommandParserPointer
    {
        /// <summary>
        /// Get a copy of the variables defined by HynusWynus
        /// <para>
        /// Environment variables cannot be unset by the user, but can be modified
        /// </para>
        /// </summary>
        public required d_GetEnvironmentVariables GetEnvironmentVariables { get; set; }

        /// <summary>
        /// Get an environment variable, or an empty string if it doesn't exist
        /// </summary>
        public required d_GetEnvironmentVariable GetEnvironmentVariable { get; set; }

        /// <summary>
        /// Get an environment variable, or null if it doesn't exist
        /// </summary>
        public required d_GetNullEnvironmentVariable GetNullEnvironmentVariable { get; set; }

        /// <summary>
        /// Set an environment variable
        /// <para>
        /// Environment variables cannot be unset by the user, but can be modified
        /// </para>
        /// </summary>
        public required d_SetEnvironmentVariable SetEnvironmentVariable { get; set; }

        /// <summary>
        /// Get a copy of the variables defined by the user
        /// </summary>
        public required d_GetUserDefinedVariables GetUserDefinedVariables { get; set; }

        /// <summary>
        /// Get a user defined variable, or an empty string if it doesn't exist
        /// </summary>
        public required d_GetUserDefinedVariable GetUserVariable { get; set; }

        /// <summary>
        /// Get a user defined variable, or null if it doesn't exist
        /// </summary>
        public required d_GetNullUserDefinedVariable GetNullUserVariable { get; set; }

        /// <summary>
        /// Set a user defined variable
        /// </summary>
        public required d_SetUserDefinedVariable SetUserVariable { get; set; }

        /// <summary>
        /// Returns a new string with Spectre.Console markup 
        /// </summary>
        public required d_MarkupCommand MarkupCommand { get; set; }

        /// <summary>
        /// All commands entered by the user
        /// </summary>
        public required d_GetCommandHistory GetCommandHistory { get; set; }


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
        public required d_WriteLine WriteLine { get; set; }

        /// <summary>
        /// Write markup text (without a newline)
        /// </summary>
        public required d_Write Write { get; set; }

        /// <summary>
        /// Create a log
        /// </summary>
        public required d_Log Log { get; set; }

        /// <summary>
        /// Create a warning log
        /// </summary>
        public required d_LogWarning LogWarning { get; set; }

        /// <summary>
        /// Create a error log
        /// </summary>
        public required d_LogError LogError { get; set; }

        /// <summary>
        /// Write a separator title
        /// </summary>
        public required d_WriteTitle WriteTitle { get; set; }

        /// <summary>
        /// Write data to the console
        /// </summary>
        public required d_WriteData WriteData { get; set; }

        /// <summary>
        /// Write a markup exception with a message
        /// </summary>
        public required d_WriteException WriteException { get; set; }

        public delegate void d_WriteLine(string line = "");
        public delegate void d_Write(string text = "");
        public delegate void d_Log(string message);
        public delegate void d_LogWarning(string log);
        public delegate void d_LogError(string log);
        public delegate void d_WriteTitle(string title);
        public delegate void d_WriteData(string header, params string[] data);
        public delegate void d_WriteException(string message, Exception exception);
    }
}