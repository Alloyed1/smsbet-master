using Microsoft.EntityFrameworkCore.Migrations;

namespace Smsbet.Web.Migrations
{
    public partial class userphone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewUserPhone",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewUserPhoneCode",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewUserPhone",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NewUserPhoneCode",
                table: "AspNetUsers");
        }
    }
}
