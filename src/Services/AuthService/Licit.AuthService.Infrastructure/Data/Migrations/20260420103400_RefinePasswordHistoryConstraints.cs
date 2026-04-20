using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licit.AuthService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefinePasswordHistoryConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordHistories_UserId",
                table: "PasswordHistories");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 10, 34, 0, 6, DateTimeKind.Utc).AddTicks(743));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 10, 34, 0, 6, DateTimeKind.Utc).AddTicks(7855));

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId_Id",
                table: "PasswordHistories",
                columns: new[] { "UserId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_UserId",
                table: "PasswordHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_UserId",
                table: "PasswordHistories");

            migrationBuilder.DropIndex(
                name: "IX_PasswordHistories_UserId_Id",
                table: "PasswordHistories");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 10, 32, 31, 328, DateTimeKind.Utc).AddTicks(957));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 20, 10, 32, 31, 328, DateTimeKind.Utc).AddTicks(8294));

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId",
                table: "PasswordHistories",
                column: "UserId");
        }
    }
}
