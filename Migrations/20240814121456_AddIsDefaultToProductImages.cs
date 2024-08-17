using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dong_Xuan_Market_Online.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDefaultToProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ProductImages");
        }
    }
}
