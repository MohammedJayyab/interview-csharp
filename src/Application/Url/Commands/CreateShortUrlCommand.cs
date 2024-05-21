using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace UrlShortenerService.Application.Url.Commands;
public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}
