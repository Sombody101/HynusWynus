using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HynusWynus.Classes;

public static class Extensions
{
    public static string PadMarkupLeft(this string input, int totalWidth)
    {
        int paddingLength = totalWidth - input.RemoveMarkup().Length;
        return paddingLength <= 0 ? input : new string(' ', paddingLength) + input;
    }

    public static string PadMarkupRight(this string input, int totalWidth)
    {
        int paddingLength = totalWidth - input.RemoveMarkup().Length;
        return paddingLength <= 0 ? input : input + new string(' ', paddingLength);
    }
}