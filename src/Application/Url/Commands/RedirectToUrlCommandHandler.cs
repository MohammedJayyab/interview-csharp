using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Exceptions;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public class RedirectToUrlCommandHandler : IRequestHandler<RedirectToUrlCommand, string>
{
    private readonly IUrlRepository _urlRepository;
    private readonly IHashids _hashids;

    public RedirectToUrlCommandHandler(IUrlRepository urlRepository, IHashids hashids)
    {
        _urlRepository = urlRepository;
        _hashids = hashids;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {
        var decodedId = DecodeSingleIdFromRequest(request);
        var url = await _urlRepository.FindUrlByIdAsync(decodedId, cancellationToken);

        if (url == null || url.OriginalUrl == null)
        {
            throw new NotFoundException($"No URL found for request Id: '{decodedId}'.");
        }

        return url.OriginalUrl;
    }

    private long DecodeSingleIdFromRequest(RedirectToUrlCommand request)
    {
        var decodedIds = _hashids.DecodeLong(request.Id);
        var decodedId = decodedIds.FirstOrDefault();

        if (decodedId == 0)
        {
            throw new NotFoundException($"Request Id: '{request.Id}' not found.");
        }

        return decodedId;
    }
}
