namespace sims.Models
{
    public class User
    {
        public int Uid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }   // email is for user login!!!
        public string PasswordHash { get; set; }
        public DateTime CreationDate { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    
    }
}