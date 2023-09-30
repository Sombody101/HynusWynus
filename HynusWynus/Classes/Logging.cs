using CommandManager;
using Spectre.Console;
using System.Text;
using Utils;

namespace HynusWynus.Classes;

internal static class Logging
{
    public static void WriteLine(string line = "")
        => AnsiConsole.MarkupLine(line);

    public static void Write(string text = "")
        => AnsiConsole.Markup(text);

    public static void Log(string log)
        => WriteLine("[blue][[!]][/]\t" + log);

    public static void LogWarning(string log)
        => WriteLine("[yellow][[!]][/]\t" + log);

    public static void LogError(string log)
        => WriteLine("[red][[!]][/]\t" + log);

    public static void WriteTitle(string title)
        => WriteTitle(new Rule(title).LeftJustified());

    public static void WriteTitle(Rule rule)
    {
        AnsiConsole.Write(rule);
        Console.Write("\r\n");
    }

    public static void WriteData(string header, params string[] data)
    {
        string text = $"{header}:\r\n";

        foreach (string d in data)
            text += "\t" + d + "\r\n";

        WriteLine(text);
    }

    public static void WriteException(string message, Exception e)
    {
        LogError(message);

        if (SettingsManager.Settings.ShowCrashErrors)
            AnsiConsole.WriteException(e);
    }

    // Read user input (But with color :0)
    internal static string ReadInput(bool forceText = false)
    {
        string prefixName = "HW";
        string prefixChar = "> ";
        int prefixLen = prefixName.Length + CommandParser.EnvironmentVariables["$PROMPTPREFIX"].Length + prefixChar.Length + 1;

        string output = "";
        int cursorPos = prefixLen;
        int commandIndex = CommandParser.CommandHistory.Count;
        CommandParser.CommandHistory.Add("");

        int line = Console.CursorTop;

        while (true)
        {
            prefixLen = prefixName.Length + CommandParser.EnvironmentVariables["$PROMPTPREFIX"].Length + prefixChar.Length + 1;
            CommandParser.CommandHistory[commandIndex] = output;
            Write(($"\r[magenta1]{prefixName}[/] [blue]{CommandParser.EnvironmentVariables["$PROMPTPREFIX"]}[/][white]{prefixChar}[/]" + CommandParser.MarkupCommand(output)).PadRight(Console.BufferWidth));

            if (output.Length + prefixLen > Console.BufferWidth)
                cursorPos = Console.BufferWidth - 1;

            Console.SetCursorPosition(cursorPos, Console.CursorTop);

            Console.CursorVisible = true;
            var key = Console.ReadKey(true);
            Console.CursorVisible = false;

            // Handle command finalization
            if (key.Key == ConsoleKey.Enter)
            {
                if (forceText)
                {
                    AnsiConsole.Write("\r\n[red]Input cannot be empty[/]");
                    return ReadInput(forceText);
                }
                else
                    break;
            }

            // Handle removing characters
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (cursorPos > prefixLen)
                {
                    output = output.Remove(cursorPos - prefixLen - 1, 1);
                    Console.Write(' ');
                    Console.SetCursorPosition(--cursorPos, Console.CursorTop);
                }
            }

            // Handle command autocompletion
            else if (key.Key == ConsoleKey.Tab)
            {
                Console.SetCursorPosition(cursorPos, Console.CursorTop);

                var trimmed = output.Trim();

                if (trimmed.Length is 0)
                    continue;

                List<string> potential = CommandParser.Commands.Where(com => com.StartsWith(trimmed)).ToList();

                if (potential.Count > 1)
                {
                    // Sort small -> large
                    potential.Sort((x, y) => y.CompareTo(x));
                    potential.Reverse();

                    if (potential.Contains(trimmed) && potential.Count > 1)
                    {
                        potential.Remove(trimmed);
                        goto PresentPossibleCommands;
                    }

                    // Check if all potential start with the same string
                    {
                        string assembled = "";

                        if (potential.Count == 0)
                            goto Finished;

                        string firstString = potential[0];
                        int minLength = firstString.Length;

                        foreach (string str in potential)
                            minLength = Math.Min(minLength, str.Length);

                        for (int i = 0; i < minLength; i++)
                        {
                            char currentChar = firstString[i];
                            foreach (string str in potential)
                                if (str[i] != currentChar)
                                {
                                    assembled = firstString[..i];
                                    goto Finished;
                                }
                        }

                        assembled = firstString[..minLength];

                    Finished:
                        if (assembled != "")
                        {
                            output = assembled;
                            cursorPos = output.Length + prefixLen;
                            continue;
                        }
                    }

                PresentPossibleCommands:
                    // List all possible commands to use
                    int longest = potential.ToArray().GetLongestLength();
                    int loop = 0;

                    var formatted = new string[potential.Count];
                    for (int i = 0; i < potential.Count; i++)
                    {
                        formatted[loop] = potential[i].PadRight(longest + 3);
                        loop++;

                        if (loop is 4)
                            loop = 0;
                    }

                    Write($"\r\n");
                    foreach (string command in formatted.Shrink())
                        AnsiConsole.Markup(CommandParser.MarkupCommand(command));
                    Write($"\r\n\r\n");
                    cursorPos = output.Length + prefixLen;
                }
                else if (potential.Count is 1)
                {
                    var command = potential[0];
                    Console.Write(command[output.Length..]);
                    output = command;
                    cursorPos = output.Length + prefixLen;
                    Console.SetCursorPosition(cursorPos, Console.CursorTop);
                }
            }

            // Allow user to change cursor position
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                if (output.Length > 0 && cursorPos > prefixLen)
                    cursorPos--;
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (output.Length > 0 && cursorPos < output.Length + prefixLen)
                    cursorPos++;
            }

            // Command history
            else if (key.Key == ConsoleKey.UpArrow)
            {
                if (commandIndex - 1 > -1)
                    output = CommandParser.CommandHistory[--commandIndex];
                Console.Write(" ".PadRight(output.Length));
                cursorPos = output.Length + prefixLen;
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (commandIndex + 1 < CommandParser.CommandHistory.Count)
                    output = CommandParser.CommandHistory[++commandIndex];
                Console.Write(" ".PadRight(output.Length));
                cursorPos = output.Length + prefixLen;
            }

            // Handle command input
            else
            {
                if (output.Length + prefixLen < Console.BufferWidth - 1)
                {
                    if (output.Length <= cursorPos - prefixLen)
                        output += key.KeyChar.ToString();
                    else
                        output = output.Insert(cursorPos - prefixLen, key.KeyChar.ToString());

                    cursorPos++;
                }
            }
        }

        CommandParser.CommandHistory.RemoveAll(string.IsNullOrEmpty);
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write("\r\n");
        Console.CursorVisible = true;

        return output.EscapeMarkup();
    }

    public static bool ReadBool(string question)
    {
        var answer = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(question).PageSize(10).AddChoices(new[] { "Yes", "No" }));

        return answer is "Yes";
    }
}
