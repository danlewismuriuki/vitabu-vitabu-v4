namespace Vitabu.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string message) : base(errorCode, message)
    {
    }

    public static NotFoundException For(string resource, object id) =>
        new($"{resource}_not_found", $"{resource} '{id}' was not found.");
}

public sealed class ConflictException : DomainException
{
    public ConflictException(string errorCode, string message) : base(errorCode, message)
    {
    }
}

public sealed class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string errorCode, string message) : base(errorCode, message)
    {
    }
}

public sealed class ValidationException : DomainException
{
    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base("validation_failed", message)
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
