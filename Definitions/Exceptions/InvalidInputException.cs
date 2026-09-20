namespace EnananV2.Definitions.Exceptions;

public class InvalidInputException(object? input, string message)
    : InvalidRequestException($"{message} Invalid input: '{input ?? "null"}'.")
{
    public object? Input { get; } = input;
}