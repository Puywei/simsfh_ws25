using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sims.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureBlacklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "BlacklistedTokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedTokens_Token",
                table: "BlacklistedTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlacklistedTokens_Token",
                table: "BlacklistedTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "BlacklistedTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);
        }
    }
}
