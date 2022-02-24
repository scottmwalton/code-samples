using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorClientSideRoleBased.Server.Data.Migrations
{
    public partial class SeedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b094a3b6-df9b-48cf-817d-ef6683e9c986", "2993a8b2-5095-49f9-9262-922eb7a38eb0", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d18feba8-7128-481f-af0c-e6ecac90520b", "ea46903d-ce93-4b11-a3a6-0492168152be", "User", "USER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b094a3b6-df9b-48cf-817d-ef6683e9c986");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d18feba8-7128-481f-af0c-e6ecac90520b");
        }
    }
}
