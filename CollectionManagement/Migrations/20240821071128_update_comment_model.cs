using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectionManagement.Migrations
{
    /// <inheritdoc />
    public partial class update_comment_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Items_ItemId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Items_ItemId",
                table: "Comments",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Items_ItemId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Items_ItemId",
                table: "Comments",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
