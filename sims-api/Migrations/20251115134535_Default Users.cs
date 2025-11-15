using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace sims.Migrations
{
    /// <inheritdoc />
    public partial class DefaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Uid", "Email", "Firstname", "Lastname", "PasswordHash", "RoleId" },
                values: new object[,]
                {
                    { 2, "max@mustermann.com", "Max", "Mustermann", "$2b$10$DHsquTrVit/1zvSpEr89Sepkj0KTKRtbxU7PJ0LejsRNUkAQPwYQy", 2 },
                    { 3, "max@musterfrau.com", "Maria", "Musterfrau", "$2b$10$qwiR8mn2wbeUxAH3SoMlo.oTtpObxaqPHhQsdp10kyF8TE7QdO9WS", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Uid",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Uid",
                keyValue: 3);
        }
    }
}
