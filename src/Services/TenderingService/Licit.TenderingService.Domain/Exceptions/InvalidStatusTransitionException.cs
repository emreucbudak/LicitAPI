namespace Licit.TenderingService.Domain.Exceptions;

public class InvalidStatusTransitionException(string from, string to)
    : Exception($"'{from}' durumundan '{to}' durumuna geçiş yapılamaz.")
{
    public string FromStatus { get; } = from;
    public string ToStatus { get; } = to;
}
