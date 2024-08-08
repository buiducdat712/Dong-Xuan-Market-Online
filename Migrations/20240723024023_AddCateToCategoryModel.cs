using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dong_Xuan_Market_Online.Migrations
{
    /// <inheritdoc />
    public partial class AddCateToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cate",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cate",
                table: "Categories");
        }
    }
}
