using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HynusWynus.Classes.Addons.Modding;

public class ModdingSpecificMethods
{
    /* CommandParser */
    public static List<string> GetCommandHistory()
        => new(CommandParser.CommandHistory);

    // Environment variables
    public static Dictionary<string, string> GetEnvironmentVars()
        => new(CommandParser.EnvironmentVariables);

    public static string GetEnvironmentVariable(string variable)
        => CommandParser.EnvironmentVariables.ContainsKey(ValidateVariable(variable)) ?
        CommandParser.EnvironmentVariables[ValidateVariable(variable)] : "";

    public static string? GetNullEnvironmentVariable(string variable)
        => CommandParser.EnvironmentVariables.ContainsKey(ValidateVariable(variable)) ?
        CommandParser.EnvironmentVariables[ValidateVariable(variable)] : null;

    public static void SetEnvironmentVariable(string variable, string value)
        => CommandParser.EnvironmentVariables[ValidateVariable(variable)] = value;

    // User defined variables
    public static Dictionary<string, string> GetUserDefinedVars()
        => new(CommandParser.UserDefinedVariables);

    public static string GetUserVariable(string variable)
        => CommandParser.UserDefinedVariables.ContainsKey(ValidateVariable(variable)) ?
        CommandParser.UserDefinedVariables[ValidateVariable(variable)] : "";

    public static string? GetNullUserVariable(string variable)
        => CommandParser.UserDefinedVariables.ContainsKey(ValidateVariable(variable)) ? 
        CommandParser.UserDefinedVariables[ValidateVariable(variable)] : null;

    public static void SetUserVariable(string variable, string value)
        => CommandParser.UserDefinedVariables[ValidateVariable(variable)] = value;

    private static string ValidateVariable(string variable)
    {
        if (variable[0] != '$')
            variable = '$' + variable;

        return variable;
    }
}
