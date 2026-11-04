using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAJobsAgent.Models;

public class Opportunity
{
    public int Id { get; set; }
    [MaxLength(150)] public string Title { get; set; } = "";
    [MaxLength(150)] public string Employer { get; set; } = "";
    /// <summary>Learnership, Internship, Bursary or Job</summary>
    [MaxLength(30)] public string Type { get; set; } = "Job";
    [MaxLength(50)] public string Province { get; set; } = "";
    [MaxLength(80)] public string City { get; set; } = "";
    [MaxLength(100)] public string MinEducation { get; set; } = "";
    [MaxLength(80)] public string Stipend { get; set; } = "";
    public DateTime? ClosingDate { get; set; }
    [MaxLength(300)] public string ApplyUrl { get; set; } = "";
    [Column(TypeName = "text")] public string Description { get; set; } = "";
    [Column(TypeName = "text")] public string Requirements { get; set; } = "";
}

public class ChatSession
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ChatMessage
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    [MaxLength(20)] public string Role { get; set; } = "user";
    [Column(TypeName = "longtext")] public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
