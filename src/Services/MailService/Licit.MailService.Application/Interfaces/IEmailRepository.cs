using Licit.MailService.Domain.Entities;

namespace Licit.MailService.Application.Interfaces;

public interface IEmailRepository
{
    Task<EmailMessage?> GetByIdAsync(Guid id);
    Task<IEnumerable<EmailMessage>> GetAllAsync(int page, int pageSize);
    Task<int> GetCountAsync();
    void Add(EmailMessage email);
    void Update(EmailMessage email);
}
