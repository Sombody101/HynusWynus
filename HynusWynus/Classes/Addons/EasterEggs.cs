using System.Security.Cryptography;
using System.Text;

namespace HynusWynus.Classes;

/*
 * Hello!
 * 
 * This class is just as the name suggests. Some easter eggs!
 * Making this app open source also makes it difficult to hide secrets without obfuscation. So let's just keep this file between us.
 * Okay?
 * 
 * You're more than welcome to message me on https://github.com/Sombody101/HynusWynus and offer some fun additions!
 */

internal static class CommandFinish
{
    public static string FC(string input)
    {
        byte[] hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(input));

        StringBuilder builder = new();
        for (int i = 0; i < hashBytes.Length; i++)
            builder.Append(hashBytes[i].ToString("X2"));

        return builder.ToString().ToLower();
    }

    public static bool Finish(string cmd)
    {
        switch (FC(cmd))
        {
            case "c1959ee80bab1495d9f709f675534f580769dfa895a75001dc264e4bc874a670501c93dd6f7c5070d8b49de7daba0fc5dcefc6e8cf6dd448654d0aafed4d64e7":
                Console.WriteLine("reached");
                return true;
        }

        return false;
    }
}
