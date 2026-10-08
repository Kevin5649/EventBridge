using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddEnquiryQuotationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicesIncluded",
                table: "Quotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TermsAndNotes",
                table: "Quotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Enquiries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                table: "Enquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredServices",
                table: "Enquiries",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Enquiries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServicesIncluded",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TermsAndNotes",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "RequiredServices",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Enquiries");
        }
    }
}
