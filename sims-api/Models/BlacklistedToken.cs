namespace sims.Models
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}