using FakeItEasy;
using FluentAssertions;
using HashidsNet;
using UrlShortenerService.Application.Common.Exceptions;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Url.Commands;
using UrlShortenerService.Domain.Entities;
using Xunit;

namespace UrlShortenerServiceTest;

public class CreateShortUrlCommandHandlerTests
{
    private readonly IUrlRepository _urlRepository;
    private readonly IHashids _hashids;
    private readonly CreateShortUrlCommandHandler _handler;
    private readonly CancellationToken _cancellationToken;

    public CreateShortUrlCommandHandlerTests()
    {
        _urlRepository = A.Fake<IUrlRepository>();
        _hashids = A.Fake<IHashids>();
        _handler = new CreateShortUrlCommandHandler(_urlRepository, _hashids);
        _cancellationToken = new CancellationToken();
    }

    [Fact]
    public async Task Handle_ShouldReturnShortUrl_WhenUrlExists()
    {
        // Arrange
        var originalUrl = "http://example-site.com";
        var existingUrl = new Url { Id = 1, OriginalUrl = originalUrl };
        var command = new CreateShortUrlCommand { Url = originalUrl };

        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).Returns(existingUrl);
        _ = A.CallTo(() => _hashids.EncodeLong(existingUrl.Id)).Returns("shortUrl");

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = result.Should().Be("shortUrl");
        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _urlRepository.AddNewUrlEntityAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldThrowShortUrlCreationException_OnException()
    {
        // Arrange
        var originalUrl = "http://example-site.com";
        var command = new CreateShortUrlCommand { Url = originalUrl };
        var exception = new Exception("Database error");

        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).Throws(exception);

        // Act
        var result = async () => await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = await result.Should().ThrowAsync<ShortUrlCreationException>()
                              .WithMessage("An error occurred while creating the short URL.");
        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _urlRepository.AddNewUrlEntityAsync(command.Url, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldCreateNewShortUrl_ForNonExistingUrl()
    {
        // Arrange
        var originalUrl = "http://new-example-site.com";
        var command = new CreateShortUrlCommand { Url = originalUrl };
        var newUrlEntity = new Url { Id = 2, OriginalUrl = originalUrl };

        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).Returns(Task.FromResult<Url?>(null));
        _ = A.CallTo(() => _urlRepository.AddNewUrlEntityAsync(command.Url, A<CancellationToken>._)).Returns(Task.FromResult(newUrlEntity));
        _ = A.CallTo(() => _hashids.EncodeLong(newUrlEntity.Id)).Returns("newShortUrl");

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = result.Should().Be("newShortUrl");
        _ = A.CallTo(() => _urlRepository.GetExistingUrlAsync(command.Url, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        _ = A.CallTo(() => _urlRepository.AddNewUrlEntityAsync(command.Url, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        _ = A.CallTo(() => _hashids.EncodeLong(newUrlEntity.Id)).MustHaveHappenedOnceExactly();
    }
}
