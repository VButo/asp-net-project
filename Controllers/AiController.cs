using API_tester.Models;
using API_tester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_tester.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/ai/request-draft")]
public class AiController : ControllerBase
{
    private readonly RequestDraftService _draftService;

    public AiController(RequestDraftService draftService)
    {
        _draftService = draftService;
    }

    [HttpPost]
    public async Task<ActionResult<AiRequestDraft>> Create(AiRequestDraftInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Prompt) || input.Prompt.Length > 2000)
        {
            return BadRequest("Prompt is required and must be at most 2000 characters.");
        }

        return Ok(await _draftService.CreateDraftAsync(input.Prompt, cancellationToken));
    }
}
