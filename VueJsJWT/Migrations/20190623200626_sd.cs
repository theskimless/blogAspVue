using Microsoft.EntityFrameworkCore.Migrations;

namespace VueJsJWT.Migrations
{
    public partial class sd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "ArticleRubrics",
                newName: "ArticleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "ArticleRubrics",
                newName: "PostId");
        }
    }
}
