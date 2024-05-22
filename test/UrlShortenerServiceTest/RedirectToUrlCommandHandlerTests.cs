using FakeItEasy;
using FluentAssertions;
using HashidsNet;
using UrlShortenerService.Application.Common.Exceptions;
using UrlShortenerService.Application.Common.Interfaces;
using UrlShortenerService.Application.Url.Commands;
using UrlShortenerService.Domain.Entities;
using Xunit;

namespace UrlShortenerServiceTest;

public class RedirectToUrlCommandHandlerTests
{
    private readonly IUrlRepository _urlRepository;
    private readonly IHashids _hashids;
    private readonly RedirectToUrlCommandHandler _handler;
    private readonly CancellationToken _cancellationToken;

    public RedirectToUrlCommandHandlerTests()
    {
        _urlRepository = A.Fake<IUrlRepository>();
        _hashids = A.Fake<IHashids>();
        _handler = new RedirectToUrlCommandHandler(_urlRepository, _hashids);
        _cancellationToken = new CancellationToken();
    }

    [Fact]
    public async Task Handle_ShouldReturnOriginalUrl_WhenValidShortUrl()
    {
        // Arrange
        var originalUrl = "http://example.com";
        var command = new RedirectToUrlCommand { Id = "validShortUrl" };
        var decodedId = 1L;
        var urlEntity = new Url { Id = decodedId, OriginalUrl = originalUrl };

        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).Returns(new long[] { decodedId });
        _ = A.CallTo(() => _urlRepository.FindUrlByIdAsync(decodedId, _cancellationToken)).Returns(Task.FromResult<Url?>(urlEntity));

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = result.Should().Be(originalUrl);
        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).MustHaveHappenedOnceExactly();
        _ = A.CallTo(() => _urlRepository.FindUrlByIdAsync(decodedId, _cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenShortUrlNotFound()
    {
        // Arrange
        var command = new RedirectToUrlCommand { Id = "invalidShortUrl" };

        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).Returns(new long[] { 0 });

        // Act
        var result = async () => await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = await result.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Request Id: '{command.Id}' not found.");
        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _urlRepository.FindUrlByIdAsync(A<long>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUrlNotFound()
    {
        // Arrange
        var command = new RedirectToUrlCommand { Id = "validShortUrl" };
        var decodedId = 1L;

        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).Returns(new long[] { decodedId });
        _ = A.CallTo(() => _urlRepository.FindUrlByIdAsync(decodedId, _cancellationToken)).Returns(Task.FromResult<Url?>(null));

        // Act
        var result = async () => await _handler.Handle(command, _cancellationToken);

        // Assert
        _ = await result.Should().ThrowAsync<NotFoundException>()
                  .WithMessage($"No URL found for request Id: '{decodedId}'.");
        _ = A.CallTo(() => _hashids.DecodeLong(command.Id)).MustHaveHappenedOnceExactly();
        _ = A.CallTo(() => _urlRepository.FindUrlByIdAsync(decodedId, _cancellationToken)).MustHaveHappenedOnceExactly();
    }
}
