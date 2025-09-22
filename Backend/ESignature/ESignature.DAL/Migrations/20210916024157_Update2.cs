using Microsoft.EntityFrameworkCore.Migrations;

namespace ESignature.DAL.Migrations
{
    public partial class Update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignerId",
                table: "ES_Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignerId",
                table: "ES_Jobs");
        }
    }
}
