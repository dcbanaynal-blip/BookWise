using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSuggestionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountSuggestionRules",
                columns: table => new
                {
                    AccountSuggestionRuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PurposeAccountId = table.Column<int>(type: "int", nullable: false),
                    PostingAccountId = table.Column<int>(type: "int", nullable: false),
                    OccurrenceCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSuggestionRules", x => x.AccountSuggestionRuleId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSuggestionRules_SellerName_PurposeAccountId_PostingAccountId",
                table: "AccountSuggestionRules",
                columns: new[] { "SellerName", "PurposeAccountId", "PostingAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSuggestionRules");
        }
    }
}
