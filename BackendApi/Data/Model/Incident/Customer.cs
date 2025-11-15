using BackendApi.Data.Model.Enum;

namespace BackendApi.Data.Model.Incident;

public class Customer
{
    public string Id { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public CustomerIsActive?  Active { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public Guid UUId { get; set; }
}