using Microsoft.EntityFrameworkCore.Migrations;

namespace ESignature.DAL.Migrations
{
    public partial class Update4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefNumber",
                table: "ES_Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefNumber",
                table: "ES_Jobs");
        }
    }
}
