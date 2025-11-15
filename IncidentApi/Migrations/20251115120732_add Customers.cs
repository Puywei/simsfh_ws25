using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestApi.Migrations
{
    /// <inheritdoc />
    public partial class addCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Incidents",
                newName: "ChangeDate");

            migrationBuilder.RenameColumn(
                name: "IncidentUUid",
                table: "Incidents",
                newName: "UUId");

            migrationBuilder.RenameColumn(
                name: "IncidentId",
                table: "Incidents",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UUId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "UUId",
                table: "Incidents",
                newName: "IncidentUUid");

            migrationBuilder.RenameColumn(
                name: "ChangeDate",
                table: "Incidents",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Incidents",
                newName: "IncidentId");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
