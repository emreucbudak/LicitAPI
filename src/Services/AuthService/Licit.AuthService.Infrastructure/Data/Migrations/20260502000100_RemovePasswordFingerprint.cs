using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licit.AuthService.Infrastructure.Data.Migrations
{
    [Migration("20260502000100_RemovePasswordFingerprint")]
    public partial class RemovePasswordFingerprint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPasswordFingerprint",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentPasswordFingerprint",
                table: "AspNetUsers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
