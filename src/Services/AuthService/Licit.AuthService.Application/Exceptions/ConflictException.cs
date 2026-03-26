namespace Licit.AuthService.Application.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message) : base(message, 409) { }
}
