using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectionManagement.Migrations
{
    /// <inheritdoc />
    public partial class updated_like_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes");

            migrationBuilder.AddColumn<int>(
                name: "LikedUserId",
                table: "Likes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_LikedUserId",
                table: "Likes",
                column: "LikedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_LikedUserId",
                table: "Likes",
                column: "LikedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_LikedUserId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_LikedUserId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "LikedUserId",
                table: "Likes");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
