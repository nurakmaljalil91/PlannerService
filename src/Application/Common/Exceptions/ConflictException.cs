namespace Application.Common.Exceptions;

/// <summary>
/// Exception that is thrown when an operation conflicts with the current state (e.g., duplicate resource).
/// </summary>
public class ConflictException : BadRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the conflict.</param>
    public ConflictException(string message) : base(message) { }
}
