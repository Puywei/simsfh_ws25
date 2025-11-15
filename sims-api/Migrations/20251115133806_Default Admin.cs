using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sims.Migrations
{
    /// <inheritdoc />
    public partial class DefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Uid", "Email", "Firstname", "Lastname", "PasswordHash", "RoleId" },
                values: new object[] { 1, "admin@admin.com", "Default", "Administrator", "$2b$10$4Xt2A0AFTwspkNlpVPZNmuZJsydGp3pfLi2.YBoGxL3T7ShwCgZLS", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Uid",
                keyValue: 1);
        }
    }
}
