using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAJobsAgent.Data;
using SAJobsAgent.Models;
using SAJobsAgent.Services;

namespace SAJobsAgent.Controllers;

[ApiController]
[Route("api")]
public class AgentController(AgentService agent, AppDbContext db) : ControllerBase
{
    public record ChatRequest(Guid? SessionId, string Message);

    [HttpPost("agent/chat")]
    public async Task<IActionResult> Chat(ChatRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 8000)
            return BadRequest(new { error = "Message is required and must be under 8000 characters." });

        var id = req.SessionId ?? Guid.NewGuid();
        if (!await db.Sessions.AnyAsync(s => s.Id == id, ct))
        {
            db.Sessions.Add(new ChatSession { Id = id });
            await db.SaveChangesAsync(ct);
        }

        var reply = await agent.ChatAsync(id, req.Message, ct);
        return Ok(new { sessionId = id, reply });
    }

    [HttpGet("opportunities")]
    public async Task<IActionResult> List([FromQuery] string? type, [FromQuery] string? province, CancellationToken ct)
    {
        var q = db.Opportunities.AsQueryable();
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(o => o.Type == type);
        if (!string.IsNullOrWhiteSpace(province)) q = q.Where(o => o.Province == province);
        return Ok(await q.OrderBy(o => o.ClosingDate).ToListAsync(ct));
    }
}
