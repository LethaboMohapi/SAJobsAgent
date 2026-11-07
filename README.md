# SAJobsAgent

I’ve built the agent as an ASP.NET Core 8 Web API with MySQL, packaged in SAJobsAgent.zip. I couldn’t compile it here because .NET isn’t installed in my environment, so run dotnet build first and send me any errors.

The agent is called Thuto. It helps South African job seekers find learnerships, internships, bursaries and jobs, improve their CVs and prepare for interviews. It follows South African conventions: no ID number or photo on a CV, NQF levels explained, and warnings about pay-to-apply scams. It replies in the user’s language when it can write it well, such as isiZulu or Afrikaans.

How it works

POST /api/agent/chat takes { sessionId?, message }. It stores the conversation in MySQL and sends the last 30 messages to Claude.
Claude has two tools, search_opportunities and get_opportunity. When it calls one, the API runs the query on your database and feeds the result back, looping up to 6 times. It is instructed to recommend only listings returned by those tools.
GET /api/opportunities is a plain endpoint for listing the data.
The code is split into Models/, Data/AppDbContext.cs, Services/AgentService.cs (the agent loop) and Controllers/AgentController.cs. The model name is a setting in appsettings.json.

To run it

Put your MySQL password in the connection string in appsettings.json.
Store your API key outside the repo: dotnet user-secrets set "ApiKey" "sk-ant-...".
Run dotnet run and open /swagger. The database and seed data are created on first start.

Before it’s useful

The 5 seeded listings are labelled SAMPLE and use placeholder links. Replace them with real data from an admin endpoint, a scraper or SETA and employer feeds.
EnsureCreated() is only for development. Move to EF migrations before production, and add authentication and rate limiting on the chat endpoint.

Next you could add a CV upload tool (PDF to text, then review), an endpoint for saving user profiles, or a React or WhatsApp front end. Which would you like first?

ASP.NET Core 8 Web API + MySQL + Claude. "Thuto" helps South African job seekers find learnerships,
internships, bursaries and jobs, improve CVs and prepare for interviews.

## Run
1. Install .NET 8 SDK and MySQL 8.
2. Set your MySQL password in `appsettings.json` (ConnectionStrings:Default).
3. Store your key outside source control:
   ```
   dotnet user-secrets init
   dotnet user-secrets set ":ApiKey" "sk-ant-..."
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
 