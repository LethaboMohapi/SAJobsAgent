using Microsoft.EntityFrameworkCore;
using SAJobsAgent.Data;
using SAJobsAgent.Services;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddHttpClient<AgentService>(c => c.Timeout = TimeSpan.FromSeconds(90));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Dev convenience: creates the database and seeds sample data. Switch to EF migrations for production.
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
