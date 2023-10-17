using Newtonsoft.Json;
using System.Text;

namespace HynusWynus.Classes.NonInteractive.HynusScript;

/*
 * Literally just a port of https://github.com/orosmatthew/hydrogen-cpp/tree/master with some modifications
 */

internal enum HScriptResult
{
    Successful,
    ScriptNotFound,
    UnidentifiedParseError,
}

internal partial class HynusScriptReader
{
    private string script = "";
    private int index = 0,
        lineNumber = 1,
        columnNumber = 1;

    public static void NewFromFilePath(string scriptPath)
    {
        var reader = new HynusScriptReader();
        reader.RunScriptFromFile(scriptPath);
    }

    public static void NewFromString(string scriptStr)
    {
        var reader = new HynusScriptReader();
        reader.RunScriptFromString(scriptStr);
    }

    public HScriptResult RunScriptFromFile(string scriptPath)
    {
        if (!File.Exists(scriptPath))
            return HScriptResult.ScriptNotFound;

        script = File.ReadAllText(scriptPath);
        var tokens = Tokenize();

        return HScriptResult.Successful;
    }

    public HScriptResult RunScriptFromString(string scriptStr)
    {
        script = scriptStr;

        var tokens = Tokenize();

        foreach (var token in tokens)
            Console.WriteLine(JsonConvert.SerializeObject(token, Formatting.Indented));

        return HScriptResult.Successful;
    }

    private enum TokenType
    {
        Keyword,
        Operator,
        Identifier,
        Literal,
        StringLiteral,
        SpecialSymbol,
        Exit,
        IntLiteral,
        Semicolon,
        OpenParenthesis,
        CloseParenthesis,
        FunctionDefinition,
        Indent,
        Let,
        Equals,
        Plus,
        Star,
        Minus,
        Compare,
        ForwardSlash,
        BackSlash,
        OpenBrace,
        CloseBrace,
        If,
        Else,
        ElseIf,
        While,
        For,
    }

    private struct Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
    }
}
