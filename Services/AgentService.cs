using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SAJobsAgent.Data;
using SAJobsAgent.Models;

namespace SAJobsAgent.Services;

/// <summary>Runs the chat model's tool calls against the opportunities stored in MySQL.</summary>
public class AgentService(HttpClient http, AppDbContext db, IConfiguration cfg)
{
    const string SystemPrompt = """
        You are Thuto, a friendly career assistant for South African job seekers, especially school leavers,
        graduates and unemployed youth. You help with: (1) finding learnerships, internships, bursaries and jobs,
        (2) writing and improving CVs and cover letters, (3) interview preparation.

        South African context you must apply:
        - Explain terms simply: learnership vs internship vs apprenticeship, NQF levels, SETAs, matric/NSC, SAQA, YES programme.
        - CV advice: 2 pages max, contact details, short profile, education (matric results/subjects), skills, experience or projects,
          3 references. Do NOT put an ID number, photo, home address or banking details on a CV. Keep it ASCII-safe and ATS friendly.
        - Be realistic about data costs and access: keep answers concise, offer plain-text CV versions.
        - Warn about scams: legitimate employers and learnerships never ask for payment, and never ask for bank PINs or OTPs.
        - Reply in the user's language if it is one you can write well (isiZulu, isiXhosa, Afrikaans, Sesotho, Setswana, etc.); otherwise use clear, simple English.

        Tool rules:
        - Use search_opportunities before recommending anything. Only recommend opportunities returned by the tools; never invent listings, closing dates or links.
        - Ask for province, highest qualification and field of interest if you need them to search.
        - If nothing matches, say so and suggest how to broaden the search.
        - You are not a lawyer or recruiter and cannot guarantee any outcome.
        """;

    static readonly JsonNode Tools = JsonNode.Parse("""
        [
          {
            "name": "search_opportunities",
            "description": "Search open learnerships, internships, bursaries and jobs. All filters are optional.",
            "input_schema": {
              "type": "object",
              "properties": {
                "keyword": { "type": "string", "description": "Skill, field or job title, e.g. 'IT', 'retail', 'C#'" },
                "type": { "type": "string", "enum": ["Learnership", "Internship", "Bursary", "Job"] },
                "province": { "type": "string", "description": "e.g. Gauteng, Western Cape, KwaZulu-Natal" }
              }
            }
          },
          {
            "name": "get_opportunity",
            "description": "Get full details of one opportunity by id.",
            "input_schema": {
              "type": "object",
              "properties": { "id": { "type": "integer" } },
              "required": ["id"]
            }
          }
        ]
        """)!;

    public async Task<string> ChatAsync(Guid sessionId, string text, CancellationToken ct)
    {
        var endpointValue = cfg["Ai:Endpoint"];
        if (!Uri.TryCreate(endpointValue, UriKind.Absolute, out var endpoint) ||
            endpoint.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Configure Ai:Endpoint as an absolute HTTPS URL.");

        var apiKey = cfg["Ai:ApiKey"];
        var model = cfg["Ai:Model"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Configure Ai:ApiKey and Ai:Model before using chat.");

        var apiKeyHeader = cfg["Ai:ApiKeyHeader"] ?? "Authorization";
        var apiKeyPrefix = cfg["Ai:ApiKeyPrefix"] ?? "Bearer ";

        db.Messages.Add(new ChatMessage { SessionId = sessionId, Role = "user", Content = text });
        await db.SaveChangesAsync(ct);

        var past = await db.Messages.Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.Id).Take(30).ToListAsync(ct);
        past.Reverse();

        var messages = new JsonArray();
        foreach (var m in past)
            messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Content });

        for (var turn = 0; turn < 6; turn++)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (!req.Headers.TryAddWithoutValidation(apiKeyHeader, apiKeyPrefix + apiKey))
                throw new InvalidOperationException("Ai:ApiKeyHeader is not a valid request header.");

            foreach (var header in cfg.GetSection("Ai:Headers").GetChildren())
            {
                if (!string.IsNullOrWhiteSpace(header.Value))
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            req.Content = JsonContent.Create(new JsonObject
            {
                ["model"] = model,
                ["max_tokens"] = 1500,
                ["system"] = SystemPrompt,
                ["tools"] = Tools.DeepClone(),
                ["messages"] = messages.DeepClone()
            });

            using var res = await http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "Assistant service returned HTTP " + (int)res.StatusCode + ".",
                    null,
                    res.StatusCode);

            var body = await res.Content.ReadAsStringAsync(ct);
            var json = JsonNode.Parse(body)!;
            var content = json["content"]!.AsArray();

            if (json["stop_reason"]?.GetValue<string>() == "tool_use")
            {
                messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content.DeepClone() });
                var results = new JsonArray();
                foreach (var block in content.Where(b => b?["type"]?.GetValue<string>() == "tool_use"))
                {
                    var output = await RunToolAsync(block!["name"]!.GetValue<string>(), block["input"]!, ct);
                    results.Add(new JsonObject
                    {
                        ["type"] = "tool_result",
                        ["tool_use_id"] = block["id"]!.GetValue<string>(),
                        ["content"] = output
                    });
                }
                messages.Add(new JsonObject { ["role"] = "user", ["content"] = results });
                continue;
            }

            var reply = string.Concat(content
                .Where(b => b?["type"]?.GetValue<string>() == "text")
                .Select(b => b!["text"]!.GetValue<string>()));

            db.Messages.Add(new ChatMessage { SessionId = sessionId, Role = "assistant", Content = reply });
            await db.SaveChangesAsync(ct);
            return reply;
        }

        return "Sorry, I couldn't finish that request. Please try again.";
    }

    async Task<string> RunToolAsync(string name, JsonNode input, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        switch (name)
        {
            case "search_opportunities":
            {
                var q = db.Opportunities.Where(o => o.ClosingDate == null || o.ClosingDate >= today);
                var type = input["type"]?.GetValue<string>();
                var province = input["province"]?.GetValue<string>();
                var kw = input["keyword"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(type)) q = q.Where(o => o.Type == type);
                if (!string.IsNullOrWhiteSpace(province))
                    q = q.Where(o => o.Province.Contains(province) || o.Province == "National");
                if (!string.IsNullOrWhiteSpace(kw))
                    q = q.Where(o => o.Title.Contains(kw) || o.Description.Contains(kw) || o.Requirements.Contains(kw));

                var rows = await q.OrderBy(o => o.ClosingDate).Take(8)
                    .Select(o => new { o.Id, o.Title, o.Employer, o.Type, o.Province, o.City, o.MinEducation, o.Stipend, o.ClosingDate })
                    .ToListAsync(ct);
                return JsonSerializer.Serialize(rows);
            }
            case "get_opportunity":
            {
                var id = input["id"]!.GetValue<int>();
                var o = await db.Opportunities.FindAsync([id], ct);
                if (o is null) return "Opportunity not found.";
                if (o.ClosingDate is not null && o.ClosingDate.Value.Date < today)
                    return "This opportunity has closed and is no longer available.";
                return JsonSerializer.Serialize(o);
            }
            default:
                return "Unknown tool: " + name;
        }
    }
}
