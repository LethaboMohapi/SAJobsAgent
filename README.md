# SAJobsAgent

ASP.NET Core 8 Web API + MySQL + Claude. "Thuto" helps South African job seekers find learnerships,
internships, bursaries and jobs, improve CVs and prepare for interviews.

## Run
1. Install .NET 8 SDK and MySQL 8.
2. Set your MySQL password in `appsettings.json` (ConnectionStrings:Default).
3. Store your Anthropic key outside source control:
   ```
   dotnet user-secrets init
   dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
   ```
4. `dotnet run`, then open `/swagger`. The database and sample data are created on first start.

## Try it
POST /api/agent/chat
```json
{ "message": "I'm a matric learner in Gauteng interested in IT. What learnerships can I apply for?" }
```
Reuse the returned `sessionId` to continue the conversation.

## Notes
- Seed listings are SAMPLE data. Replace them with real listings (admin endpoint, scraper or SETA/employer feed).
- Swap `EnsureCreated()` for EF migrations before production, and add auth + rate limiting on /api/agent/chat.
