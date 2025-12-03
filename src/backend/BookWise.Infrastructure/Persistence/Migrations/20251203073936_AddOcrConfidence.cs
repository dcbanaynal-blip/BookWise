using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOcrConfidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OcrConfidence",
                table: "Receipts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "ReceiptLineItems",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OcrConfidence",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "ReceiptLineItems");
        }
    }
}
