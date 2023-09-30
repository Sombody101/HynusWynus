namespace HynusWynusModdingAPI.Shared.Exceptions;

public class InvalidCommandData : Exception
{
    public InvalidCommandData(string message)
        : base(message) { }
}

public class InvalidVersionStringFormat : Exception
{
    public InvalidVersionStringFormat(string message)
        : base(message) { }
}