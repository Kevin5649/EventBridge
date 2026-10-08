using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannerEventTypesAndEnquiryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Enquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlannerEventTypes",
                columns: table => new
                {
                    PlannerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannerEventTypes", x => new { x.PlannerId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PlannerEventTypes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlannerEventTypes_EventPlanners_PlannerId",
                        column: x => x.PlannerId,
                        principalTable: "EventPlanners",
                        principalColumn: "PlannerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_CategoryId",
                table: "Enquiries",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlannerEventTypes_CategoryId",
                table: "PlannerEventTypes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Categories_CategoryId",
                table: "Enquiries",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Categories_CategoryId",
                table: "Enquiries");

            migrationBuilder.DropTable(
                name: "PlannerEventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_CategoryId",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Enquiries");
        }
    }
}
