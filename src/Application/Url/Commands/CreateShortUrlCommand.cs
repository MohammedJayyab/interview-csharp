using FluentValidation;
using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Exceptions;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IUrlRepository _urlRepository;
    private readonly IHashids _hashids;

    public CreateShortUrlCommandHandler(IUrlRepository urlRepository, IHashids hashids)
    {
        _urlRepository = urlRepository;
        _hashids = hashids;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingUrl = await _urlRepository.GetExistingUrlAsync(request.Url, cancellationToken);

            if (existingUrl != null)
            {
                return _hashids.EncodeLong(existingUrl.Id);
            }

            var newUrlEntity = await _urlRepository.AddNewUrlEntityAsync(request.Url, cancellationToken);

            return _hashids.EncodeLong(newUrlEntity.Id);
        }
        catch (Exception)
        {
            throw new ShortUrlCreationException("An error occurred while creating the short URL.");
        }
    }
}
