using MediatR;

namespace UrlShortenerService.Application.Url.Commands;
public record RedirectToUrlCommand : IRequest<string>
{
    public string Id { get; init; } = default!;
}
