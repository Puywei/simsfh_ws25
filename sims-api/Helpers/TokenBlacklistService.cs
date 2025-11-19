using sims.Data;
using sims.Models;

namespace sims.Helpers
{
    public static class TokenBlacklistService
    {
        public static async Task BlacklistToken(UserDbContext db, string token, DateTime expiry)
        {
            db.BlacklistedTokens.Add(new BlacklistedToken
            {
                Token = token,
                Expiry = expiry
            });
            await db.SaveChangesAsync();
        }
    }
}