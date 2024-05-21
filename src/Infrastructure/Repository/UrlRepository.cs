using System.Threading;
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
        var urlEntityExists = await _context.Urls.AnyAsync(u => u.OriginalUrl.Equals(url), cancellationToken);

        if (urlEntityExists)
        {
            return await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl.Equals(url), cancellationToken);
        }

        return null;
    }

    public async Task<Domain.Entities.Url?> FindUrlByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Urls.FindAsync(id, cancellationToken);
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
