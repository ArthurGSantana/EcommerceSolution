using System.Text;
using System.Text.Json;
using EcommerceMinified.Application.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EcommerceMinified.Tests.Application.Caching;

public class RedisServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<RedisService>> _loggerMock;
    private readonly RedisService _sut;

    public RedisServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisService>>();
        _sut = new RedisService(_loggerMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenCacheHit_ShouldReturnDeserializedObject()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var testObject = new TestData { Name = "Test", Value = 123 };
        var serialized = JsonSerializer.Serialize(testObject);

        _cacheMock.Setup(c => c.GetAsync($"TestData_{testId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(serialized));

        // Act
        var result = await _sut.GetAsync<TestData>(testId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    [Fact]
    public async Task GetAsync_WhenCacheMiss_ShouldReturnDefault()
    {
        // Arrange
        var testId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(byte[]));

        // Act
        var result = await _sut.GetAsync<TestData>(testId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenExceptionThrown_ShouldLogAndReturnDefault()
    {
        // Arrange
        var testId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        // Act
        var result = await _sut.GetAsync<TestData>(testId);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenSuccessful_ShouldCallCacheWithCorrectParameters()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var testObject = new TestData { Name = "Test", Value = 123 };

        // Act
        await _sut.SetAsync(testId, testObject);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            $"TestData_{testId}",
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains("Test") && Encoding.UTF8.GetString(b).Contains("123")),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldUseProvidedExpiration()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var testObject = new TestData { Name = "Test", Value = 123 };
        var customExpiration = TimeSpan.FromMinutes(10);

        DistributedCacheEntryOptions? capturedOptions = null;

        _cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, options, token) => capturedOptions = options)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.SetAsync(testId, testObject, customExpiration);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(customExpiration);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallCacheWithCorrectKey()
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        await _sut.RemoveAsync<TestData>(testId);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync($"TestData_{testId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Teste especial para o circuit breaker
    [Fact]
    public async Task CircuitBreaker_ShouldOpen_AfterMultipleFailures()
    {
        // Arrange
        var testId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        // Act - causando falhas repetidas
        for (int i = 0; i < 6; i++)
        {
            await _sut.GetAsync<TestData>(testId);
        }

        // Reset mock para poder verificar se o circuit breaker está aberto
        _cacheMock.Invocations.Clear();

        // Tentar chamada após circuito aberto
        await _sut.GetAsync<TestData>(testId);

        // Assert - o cache não deve ser chamado quando o circuito está aberto
        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}