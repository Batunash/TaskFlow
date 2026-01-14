using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class uniquenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Name",
                table: "Projects",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_OrganizationId_Name",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId",
                table: "Projects",
                column: "OrganizationId");
        }
    }
}
