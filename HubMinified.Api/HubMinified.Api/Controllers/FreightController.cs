using System;
using HubMinified.Domain.Interfaces.Services;
using HubMinified.Domain.ViewModel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HubMinified.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FreightController(IFreightService _freightService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CalculateFreightAsync(FreightRequestDto freightRequest)
    {
        var response = await _freightService.CalculateFreightAsync(freightRequest);
        return Ok(response);
    }
}
