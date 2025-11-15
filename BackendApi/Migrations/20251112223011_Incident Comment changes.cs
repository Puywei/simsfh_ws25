using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Migrations
{
    /// <inheritdoc />
    public partial class IncidentCommentchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentComments_CommentId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_CommentId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Incidents");

            migrationBuilder.AlterColumn<string>(
                name: "IncidentId",
                table: "IncidentComments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentComments_IncidentId",
                table: "IncidentComments",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentComments_Incidents_IncidentId",
                table: "IncidentComments",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentComments_Incidents_IncidentId",
                table: "IncidentComments");

            migrationBuilder.DropIndex(
                name: "IX_IncidentComments_IncidentId",
                table: "IncidentComments");

            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IncidentId",
                table: "IncidentComments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CommentId",
                table: "Incidents",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentComments_CommentId",
                table: "Incidents",
                column: "CommentId",
                principalTable: "IncidentComments",
                principalColumn: "CommentId");
        }
    }
}
