# SAJobsAgent

SAJobsAgent is an ASP.NET Core 8 Web API backed by MySQL. Its assistant, Thuto, helps South African job seekers find learnerships, internships, bursaries and jobs, improve CVs, and prepare for interviews.

The guidance follows South African conventions: no ID number or photo on a CV, simple NQF explanations, and warnings about pay-to-apply scams. Thuto replies in the user's language when it can do so well, including isiZulu, isiXhosa, Afrikaans, Sesotho and Setswana.

## How it works

- POST /api/agent/chat accepts a sessionId (optional) and message, stores the conversation in MySQL, and sends the latest 30 messages to a configured tool-capable chat endpoint.
- The assistant can call search_opportunities and get_opportunity. The API runs each query against MySQL and returns the results for the assistant to use.
- GET /api/opportunities lists open listings. Optional type and province query parameters filter the results.
- Swagger documentation is available at /swagger. The page footer credits the author.

The configured endpoint must accept a JSON messages request with model, system, max_tokens and tools fields, and return content blocks with a stop_reason. It must support the tool call structure used by this project.

## Run

1. Install the .NET 8 SDK and MySQL 8.
2. Set the MySQL password in appsettings.json under ConnectionStrings:Default.
3. Configure the chat endpoint and credentials using .NET user secrets:

       dotnet user-secrets set "Ai:Endpoint" "https://your-chat-endpoint.example/v1/messages"
       dotnet user-secrets set "Ai:ApiKey" "your-secret-key"
       dotnet user-secrets set "Ai:Model" "your-tool-capable-model"

   The default key header is Authorization with the Bearer prefix. If your endpoint requires different authentication, set Ai:ApiKeyHeader and Ai:ApiKeyPrefix. Additional request headers can be supplied under Ai:Headers. Keep keys and sensitive header values out of source control.

4. Run dotnet run and open /swagger. The database and sample data are created on first start.

## Try it

POST /api/agent/chat

    {
      "message": "I'm a matric learner in Gauteng interested in IT. What learnerships can I apply for?"
    }

Reuse the returned sessionId to continue the conversation.

## Notes

- The five seeded listings are SAMPLE data with placeholder links. Replace them with real listings from an admin endpoint, scraper, SETA portal or employer feed.
- EnsureCreated() is for development. Use EF migrations before production, and add authentication and rate limiting to the chat endpoint.
- Chat requests are limited to 8,000 characters. Requests to unavailable or unsuccessful chat endpoints return a generic 502 response.

The code is organized into Models/, Data/AppDbContext.cs, Services/AgentService.cs and Controllers/AgentController.cs.
