using System.Text;

namespace HynusWynus.Classes.NonInteractive.HynusScript;

internal partial class HynusScriptReader
{
    private List<Token> Tokenize()
    {
        List<Token> tokens = new();

        StringBuilder buff = new();
        char? letter;
        while ((letter = Peek()) is not null)
        {
            switch (letter)
            {
                case char l when char.IsLetter(l):
                    buff.Append(Consume());

                    char? subLetter;
                    while ((subLetter = Peek()) is not null && char.IsLetter((char)subLetter))
                        buff.Append(Consume());

                    string assembled = buff.ToString();

                    switch (assembled)
                    {
                        case "exit":
                            tokens.Add(new() { Type = TokenType.Exit, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "let":
                        case "var":
                            tokens.Add(new() { Type = TokenType.Let, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "if":
                            tokens.Add(new() { Type = TokenType.If, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "else":
                            tokens.Add(new() { Type = TokenType.Else, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "elif":
                            tokens.Add(new() { Type = TokenType.ElseIf, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "while":
                            tokens.Add(new() { Type = TokenType.While, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "for":
                            tokens.Add(new() { Type = TokenType.For, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "fun":
                        case "func":
                        case "function":
                            tokens.Add(new() { Type = TokenType.FunctionDefinition, LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;

                        case "usrvar":
                        case "envar":
                            tokens.Add(new() { Type = TokenType.If, LineNumber = lineNumber, ColumnNumber = columnNumber, Value = assembled });
                            break;

                        default:
                            tokens.Add(new() { Type = TokenType.Indent, Value = buff.ToString(), LineNumber = lineNumber, ColumnNumber = columnNumber });
                            break;
                    }

                    buff.Clear();
                    break;

                case char n when char.IsNumber(n):
                    buff.Append(Consume());

                    char? subLetter2;
                    while ((subLetter2 = Peek()) is not null && char.IsNumber((char)subLetter2))
                        buff.Append(Consume());

                    tokens.Add(new() { Type = TokenType.IntLiteral, Value = buff.ToString() });
                    buff.Clear();
                    break;

                // Only required for function definitions
                case '(':
                    Consume();
                    tokens.Add(new() { Type = TokenType.OpenParenthesis, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case ')':
                    Consume();
                    tokens.Add(new() { Type = TokenType.CloseParenthesis, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;
                //

                case ';':
                    Consume();
                    tokens.Add(new() { Type = TokenType.Semicolon, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '=':
                    Consume();
                    if (Peek() == '=')
                    {
                        Consume();
                        tokens.Add(new() { Type = TokenType.Compare, LineNumber = lineNumber, ColumnNumber = columnNumber, Value = "==" });
                    }
                    else
                        tokens.Add(new() { Type = TokenType.Equals, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '+':
                    Consume();
                    tokens.Add(new() { Type = TokenType.Plus, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '*':
                    Consume();
                    tokens.Add(new() { Type = TokenType.Star, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '-':
                    Consume();
                    tokens.Add(new() { Type = TokenType.Minus, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '/':
                    Consume();
                    tokens.Add(new() { Type = TokenType.ForwardSlash, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '{':
                    Consume();
                    tokens.Add(new() { Type = TokenType.OpenBrace, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '}':
                    Consume();
                    tokens.Add(new() { Type = TokenType.CloseBrace, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case '"':
                    Consume();
                    buff.Append('"');

                    while ((letter = Peek()) is not null && letter != '"')
                        if (letter == '\\')
                        {
                            buff.Append(Consume());
                            char? escapedChar = Peek();
                            if (escapedChar != null)
                            {
                                buff.Append(Consume());
                                buff.Append(escapedChar);
                            }
                        }
                        else
                            buff.Append(Consume());

                    if (letter == '"')
                    {
                        buff.Append(Consume());
                        tokens.Add(new() { Type = TokenType.StringLiteral, Value = buff.ToString(), LineNumber = lineNumber, ColumnNumber = columnNumber });
                    }
                    else
                    {
                        Logging.LogError($"Unterminated string literal at line {lineNumber}, column {columnNumber}");
                        return new();
                    }

                    buff.Clear();
                    break;

                case '\'':
                    Consume();
                    buff.Append('\'');

                    while ((letter = Peek()) is not null && letter != '\'')
                        buff.Append(Consume());

                    if (letter == '\'')
                    {
                        buff.Append(Consume());
                        tokens.Add(new() { Type = TokenType.StringLiteral, Value = buff.ToString(), LineNumber = lineNumber, ColumnNumber = columnNumber });
                    }
                    else
                    {
                        Logging.LogError($"Unterminated single-quoted literal at line {lineNumber}, column {columnNumber}");
                        return new List<Token>();
                    }

                    buff.Clear();
                    break;

                case '\\':
                    Consume();
                    tokens.Add(new() { Type = TokenType.BackSlash, LineNumber = lineNumber, ColumnNumber = columnNumber });
                    break;

                case char ws when char.IsWhiteSpace(ws):
                    Consume();
                    break;

                default:
                    Logging.LogError($"Parse error at position {columnNumber} in line {lineNumber} ({index}) : issue item: `{letter}`");
                    return new List<Token>();
            }

            //Consume();
        }

        return tokens;
    }

    private char? Peek(int offset = 0)
    {
        if (index + offset >= script.Length)
            return null;

        return script[index + offset];
    }

    private char Consume()
    {
        if (Peek() == '\n')
        {
            lineNumber++;
            columnNumber = 1;
        }
        else
            columnNumber++;

        return script[index++];
    }
}
