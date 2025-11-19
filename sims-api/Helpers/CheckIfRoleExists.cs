using Microsoft.EntityFrameworkCore;
using sims.Data;

namespace sims.Helpers
{
    public static class RoleHelper
    {
        public static async Task<bool> CheckIfRoleExists(UserDbContext db, int roleId)
        {
            return await db.Roles.AnyAsync(r => r.RoleId == roleId);
        }
    }
}