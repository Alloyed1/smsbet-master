using Microsoft.EntityFrameworkCore.Migrations;

namespace Smsbet.Web.Migrations
{
    public partial class sdfgsdfgsdfg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmPasswordCode",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewConfirmPassword",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPasswordCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NewConfirmPassword",
                table: "AspNetUsers");
        }
    }
}
