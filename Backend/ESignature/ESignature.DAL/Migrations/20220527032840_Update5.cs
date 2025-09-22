using Microsoft.EntityFrameworkCore.Migrations;

namespace ESignature.DAL.Migrations
{
    public partial class Update5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageSign",
                table: "ES_Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisiblePosition",
                table: "ES_Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageSign",
                table: "ES_Jobs");

            migrationBuilder.DropColumn(
                name: "VisiblePosition",
                table: "ES_Jobs");
        }
    }
}
