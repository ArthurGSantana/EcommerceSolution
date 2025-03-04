using System;
using EcommerceMinified.Domain.Config;
using EcommerceMinified.Domain.Interfaces.RestRepository;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;
using RestSharp;

namespace EcommerceMinified.Data.Rest.Repository;

public class HubMinifiedRestRespository(ILogger<HubMinifiedRestRespository> _logger, IOptions<EcommerceMinifiedConfig> _options, ResiliencePipelineProvider<string> _pipelineProvider) : BaseRestRepository(_logger, _options.Value.HubMinifiedBaseUrl, _pipelineProvider), IHubMinifiedRestRespository
{
    public async Task<FreightResponseDto> GetFreightInfoAsync(FreightRequestDto freightRequest)
    {
        var request = new RestRequest("freight", Method.Post);
        request.AddJsonBody(freightRequest);

        return await ExecuteOrThrowAsync<FreightResponseDto>(request, true);
    }
}
