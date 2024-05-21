using FluentValidation;

namespace UrlShortenerService.Application.Url.Commands;

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    private const int MAX_URL_LENGTH = 2000;

    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
             .NotEmpty()
             .WithMessage("Url is required.")

             .Must(IsValidUrl)
             .WithMessage("Invalid URL format.")

             .MaximumLength(MAX_URL_LENGTH)
             .WithMessage($"URL is too long. Maximum length is '{MAX_URL_LENGTH}' characters.")

             .Must(HasValidScheme)
             .WithMessage("URL must start with a valid scheme like: (http, https).");

        /*.Must(NoSpecialCharacters)
          .WithMessage("URL contains invalid characters.");
        */
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private bool HasValidScheme(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }

        return false;
    }

    private bool NoSpecialCharacters(string url)
    {
        var disallowedCharacters = new[]
        {
                ' ', '<', '>', '"', '{', '}', '|', '\\', '^', '`'
        };

        return !url.Any(c => disallowedCharacters.Contains(c));
    }
}
