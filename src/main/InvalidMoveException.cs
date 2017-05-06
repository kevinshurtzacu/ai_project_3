using System.Runtime.Serialization;
using System;

public class InvalidMoveException : Exception
{
    public InvalidMoveException()
        : base("Error: Selected move does not exist among the options listed")
    {}

    public InvalidMoveException(int index)
        : base($"Error: Move {index} does not exist among the options listed")
    {}

    public InvalidMoveException(string message)
        : base(message)
    {}

    public InvalidMoveException(string message, Exception inner)
        : base(message, inner)
    {}
}