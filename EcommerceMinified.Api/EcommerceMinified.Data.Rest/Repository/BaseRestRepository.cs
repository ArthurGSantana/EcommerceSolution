using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using RestSharp;

namespace EcommerceMinified.Data.Rest.Repository;

public class BaseRestRepository
{
    protected readonly ILogger Logger;
    protected readonly RestClient Client;
    public EventHandler? OnUnauthorizedResponse;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly string _resilienceKeyDefault = "default";

    public BaseRestRepository(ILogger logger, string baseUrl, ResiliencePipelineProvider<string> pipelineProvider)
    {
        Logger = logger;
        _pipelineProvider = pipelineProvider;

        Client = new RestClient(new RestClientOptions(baseUrl));
    }

    protected async Task<T> ExecuteOrThrowAsync<T>(RestRequest request, bool throwException = false, string? resilienceKey = null)
    {
        resilienceKey ??= _resilienceKeyDefault;

        var pipeline = _pipelineProvider.GetPipeline(resilienceKey);

        var responseData = default(T);

        try
        {
            responseData = await pipeline.ExecuteAsync(async (_) =>
            {
                var response = await Client.ExecuteAsync<T>(request, CancellationToken.None);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    OnUnauthorizedResponse?.Invoke(response, EventArgs.Empty);
                }

                if (response.ErrorException != null && throwException)
                {
                    throw new InvalidOperationException(
                        $"Parse error of the BaseRestRepository response  ({request.Resource}): {response.StatusCode} - {response.Content}",
                        response.ErrorException);
                }

                if (!response.IsSuccessful && throwException)
                {
                    throw new HttpRequestException(
                        $"Response error of BaseRestRepository ({request.Resource}): {response.StatusCode} - {response.Content}",
                        null, response.StatusCode);
                }

                return response.Data!;
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on BaseRestRepository.ExecuteOrThrowAsync");
        }

        return responseData ?? default!;
    }
}
