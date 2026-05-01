namespace Licit.TenderingService.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
