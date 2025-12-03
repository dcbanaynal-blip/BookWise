using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptOcrExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ReceiptProcessingJobs",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ReceiptProcessingJobs");
        }
    }
}
