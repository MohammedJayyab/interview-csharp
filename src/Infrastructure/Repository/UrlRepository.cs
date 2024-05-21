using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Infrastructure.Repository;

public class UrlRepository : IUrlRepository
{
    private readonly IApplicationDbContext _context;

    public UrlRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Url?> GetExistingUrlAsync(string url, CancellationToken cancellationToken)
    {
        return await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl == url, cancellationToken);
    }

    public async Task<Domain.Entities.Url> AddNewUrlEntityAsync(string url, CancellationToken cancellationToken)
    {
        var urlEntity = new Domain.Entities.Url
        {
            OriginalUrl = url
        };

        _ = await _context.Urls.AddAsync(urlEntity, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return urlEntity;
    }
}
