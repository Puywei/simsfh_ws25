using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sims_web_app.Data.Models;

public class IncidentResponder
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public Guid Id { get; set; }
}