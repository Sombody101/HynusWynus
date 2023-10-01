namespace HynusWynusModdingAPI.Shared.Exceptions;

public class InvalidCommandDataException : Exception
{
    public InvalidCommandDataException(string message)
        : base(message) { }
}

public class InvalidVersionStringFormatException : Exception
{
    public InvalidVersionStringFormatException(string message)
        : base(message) { }
}