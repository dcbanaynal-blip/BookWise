using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeExternalAccountNumberOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_ExternalAccountNumber",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountNumber",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ExternalAccountNumber",
                table: "Accounts",
                column: "ExternalAccountNumber",
                unique: true,
                filter: "[ExternalAccountNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_ExternalAccountNumber",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountNumber",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ExternalAccountNumber",
                table: "Accounts",
                column: "ExternalAccountNumber",
                unique: true);
        }
    }
}
