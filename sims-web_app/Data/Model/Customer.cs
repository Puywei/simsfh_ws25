using System.ComponentModel.DataAnnotations;
using sims_web_app.Data.Model.Enum;

namespace sims_web_app.Data.Model;


public class Customer
{
    public string Id { get; set; }
    [StringLength(100)]
    public string CompanyName { get; set; }
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        ErrorMessage = "Ungültige E-Mail-Adresse")]
    public string Email { get; set; }
    [RegularExpression(@"^\+[0-9]+$",
        ErrorMessage = "Telefonnummer muss mit + beginnen und nur Zahlen enthalten")]
    public string? PhoneNumber { get; set; }
    [StringLength(150)]
    public string? Address { get; set; }
    [StringLength(50)]
    public string? City { get; set; }
    [StringLength(50)]
    public string? State { get; set; }
    [StringLength(12)]
    public string? ZipCode { get; set; }
    [StringLength(60)]
    public string? Country { get; set; }
    public CustomerIsActive? Active { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public Guid? UUId { get; set; }
}