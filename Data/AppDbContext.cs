using Microsoft.EntityFrameworkCore;
using SAJobsAgent.Models;

namespace SAJobsAgent.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<ChatSession> Sessions => Set<ChatSession>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ChatMessage>().HasOne<ChatSession>().WithMany().HasForeignKey(m => m.SessionId);
        b.Entity<ChatMessage>().HasIndex(m => m.SessionId);

        // SAMPLE DATA ONLY: replace with real listings (SETA portals, employer feeds, admin form).
        b.Entity<Opportunity>().HasData(
            new Opportunity { Id = 1, Title = "IT Systems Development Learnership (NQF 5)", Employer = "SAMPLE Tech Academy", Type = "Learnership", Province = "Gauteng", City = "Johannesburg", MinEducation = "Matric with Maths or Maths Lit", Stipend = "R4 500 / month", ClosingDate = new DateTime(2027, 3, 31), ApplyUrl = "https://example.org/apply/1", Description = "12-month learnership combining classroom training and workplace experience in software development.", Requirements = "Matric, age 18-35, SA citizen, unemployed, interest in programming" },
            new Opportunity { Id = 2, Title = "Retail Management Learnership (NQF 4)", Employer = "SAMPLE Retail Group", Type = "Learnership", Province = "Western Cape", City = "Cape Town", MinEducation = "Matric", Stipend = "R3 800 / month", ClosingDate = new DateTime(2027, 2, 28), ApplyUrl = "https://example.org/apply/2", Description = "Store-based learnership covering customer service, stock control and team leading.", Requirements = "Matric, customer service skills, willing to work weekends" },
            new Opportunity { Id = 3, Title = "Graduate IT Support Internship", Employer = "SAMPLE Municipality", Type = "Internship", Province = "KwaZulu-Natal", City = "Durban", MinEducation = "National Diploma or degree in IT", Stipend = "R6 000 / month", ClosingDate = new DateTime(2027, 4, 15), ApplyUrl = "https://example.org/apply/3", Description = "24-month internship supporting municipal IT infrastructure and helpdesk.", Requirements = "IT diploma/degree, no prior work experience, valid driver's licence an advantage" },
            new Opportunity { Id = 4, Title = "Junior C# Developer", Employer = "SAMPLE Software House", Type = "Job", Province = "Gauteng", City = "Pretoria", MinEducation = "Diploma or degree in IT / Software Development", Stipend = "Market related", ClosingDate = new DateTime(2027, 1, 31), ApplyUrl = "https://example.org/apply/4", Description = "Build and maintain ASP.NET Core APIs and MySQL-backed applications with a small agile team.", Requirements = "C#, SQL, Git, REST APIs; GitHub portfolio preferred" },
            new Opportunity { Id = 5, Title = "Engineering Bursary", Employer = "SAMPLE Energy Utility", Type = "Bursary", Province = "National", City = "Various", MinEducation = "Matric with Maths and Physical Science", Stipend = "Covers tuition and accommodation", ClosingDate = new DateTime(2027, 5, 31), ApplyUrl = "https://example.org/apply/5", Description = "Full bursary for engineering students with work-back obligation.", Requirements = "Matric 65%+, financial need, SA citizen" }
        );
    }
}
