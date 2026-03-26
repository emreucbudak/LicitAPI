using Licit.MailService.Application.Interfaces;
using Licit.MailService.Domain.Entities;
using Licit.MailService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.MailService.Infrastructure.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly MailDbContext _context;

    public EmailRepository(MailDbContext context) => _context = context;

    public async Task<EmailMessage?> GetByIdAsync(Guid id) =>
        await _context.EmailMessages.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<EmailMessage>> GetAllAsync(int page, int pageSize) =>
        await _context.EmailMessages
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public void Add(EmailMessage email) => _context.EmailMessages.Add(email);

    public void Update(EmailMessage email) => _context.EmailMessages.Update(email);
}
