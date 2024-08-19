using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dong_Xuan_Market_Online.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorySubModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng CategorySubModel
            migrationBuilder.CreateTable(
                name: "CategorySubModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySubModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySubModels_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade); // Hoặc NoAction tùy thuộc vào yêu cầu của bạn
                });

            // Thêm cột CategorySubId vào bảng ProductModel
            migrationBuilder.AddColumn<int>(
                name: "CategorySubId",
                table: "Products",
                type: "int",
                nullable: true);

            // Thêm khóa ngoại CategorySubId
            migrationBuilder.AddForeignKey(
                name: "FK_Products_CategorySubModels_CategorySubId",
                table: "Products",
                column: "CategorySubId",
                principalTable: "CategorySubModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // Hoặc NoAction tùy thuộc vào yêu cầu của bạn
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa cột CategorySubId khỏi bảng ProductModel
            migrationBuilder.DropForeignKey(
                name: "FK_Products_CategorySubModels_CategorySubId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategorySubId",
                table: "Products");

            // Xóa bảng CategorySubModel
            migrationBuilder.DropTable(
                name: "CategorySubModels");
        }
    }
}
