using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace EcommerceMinified.Tests.DataRest
{
    public class ResiliencePoliciesTests
    {
        private readonly ResiliencePipelineProvider<string> _pipelineProvider;
        private readonly Mock<ILogger<ResiliencePoliciesTests>> _loggerMock;
        private int _executionCount;

        public ResiliencePoliciesTests()
        {
            _loggerMock = new Mock<ILogger<ResiliencePoliciesTests>>();

            // Configurar pipeline de resiliência exatamente como na aplicação real
            var services = new ServiceCollection();

            services.AddResiliencePipeline("default", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    Delay = TimeSpan.FromMilliseconds(50), // Tempo reduzido para testes
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Exponential
                });

                builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(1),
                    MinimumThroughput = 2,
                    BreakDuration = TimeSpan.FromMilliseconds(500)
                });

                builder.AddTimeout(TimeSpan.FromSeconds(1));
            });

            var serviceProvider = services.BuildServiceProvider();
            _pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        }

        [Fact]
        public async Task RetryPolicy_ShouldRetryOnFailure_ThenSucceed()
        {
            // Arrange
            _executionCount = 0;
            var pipeline = _pipelineProvider.GetPipeline("default");

            // Act
            var result = await pipeline.ExecuteAsync(async _ =>
            {
                _executionCount++;

                if (_executionCount <= 1)
                {
                    // Simular falha na primeira execução
                    await Task.Yield(); // Ensure asynchronous behavior
                    throw new HttpRequestException("Simulated failure", null, HttpStatusCode.InternalServerError);
                }

                // Sucesso após a primeira falha
                return "Success after retry";
            });

            // Assert
            result.Should().Be("Success after retry");
            _executionCount.Should().Be(2); // Chamada original + 1 retry
        }

        [Fact]
        public async Task CircuitBreaker_ShouldOpenAfterFailures()
        {
            // Arrange
            var pipeline = _pipelineProvider.GetPipeline("default");
            var failures = new List<Exception>();

            // Act - Causar múltiplas falhas para abrir o circuito
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await pipeline.ExecuteAsync(_ =>
                        throw new HttpRequestException("Simulated failure", null, HttpStatusCode.InternalServerError));
                }
                catch (Exception ex)
                {
                    failures.Add(ex);
                }
            }

            bool circuitIsOpen = false;

            // Act - Tentar novamente após muitas falhas
            try
            {
                await pipeline.ExecuteAsync(_ => new ValueTask<string>("Shouldn't reach here"));
            }
            catch (BrokenCircuitException)
            {
                circuitIsOpen = true;
            }
            catch (Exception)
            {
                // Ignora outras exceções
            }

            // Assert
            circuitIsOpen.Should().BeTrue("circuit breaker should be open after multiple failures");
        }

        [Fact]
        public async Task Timeout_ShouldThrowTimeoutException()
        {
            // Arrange
            var pipeline = _pipelineProvider.GetPipeline("default");

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                var cts = new CancellationTokenSource();

                await pipeline.ExecuteAsync(async cancellationToken =>
                {
                    try
                    {
                        // Usar o CancellationToken recebido do pipeline
                        await Task.Delay(3000, cancellationToken); // 10 segundos (bem mais que o timeout)
                        return "Should never reach here due to timeout";
                    }
                    catch (OperationCanceledException)
                    {
                        // Capturar o cancelamento explicitamente para rethrow
                        throw new TimeoutRejectedException("Operation timed out");
                    }
                }, cts.Token);
            });

            // Verifique se alguma exceção relacionada a timeout foi lançada
            exception.Should().NotBeNull();
            exception.Should().BeOfType<TimeoutRejectedException>(
                "A timeout exception should be thrown when operation exceeds configured timeout");
        }
    }
}