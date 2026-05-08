using System;
using Licit.WalletService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licit.WalletService.Infrastructure.Data.Migrations
{
    [DbContext(typeof(WalletDbContext))]
    [Migration("20260508124000_AddWalletDepositPayments")]
    public partial class AddWalletDepositPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletDepositPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ClientIdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WalletTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletDepositPayments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletDepositPayments_StripePaymentIntentId",
                table: "WalletDepositPayments",
                column: "StripePaymentIntentId",
                unique: true,
                filter: "\"StripePaymentIntentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletDepositPayments_UserId_ClientIdempotencyKey",
                table: "WalletDepositPayments",
                columns: new[] { "UserId", "ClientIdempotencyKey" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletDepositPayments");
        }
    }
}
