using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortenerService.Application.Common.Interfaces;

public interface IUrlRepository
{
    Task<Domain.Entities.Url?> GetExistingUrlAsync(string url, CancellationToken cancellationToken);

    Task<Domain.Entities.Url> AddNewUrlEntityAsync(string url, CancellationToken cancellationToken);
}
