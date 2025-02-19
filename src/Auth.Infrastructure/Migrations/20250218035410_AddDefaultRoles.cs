using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "78d18257-e588-4610-a208-13711f796bce", "3e2de886-f836-4c17-917e-4060ba454d17", "Standard user role", "User", "USER" },
                    { "7bc2c10b-f14c-4308-bebd-52351db560ae", "88b50ff1-ed31-4d00-aed6-86125f9de22b", "Administrator role with full access", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "78d18257-e588-4610-a208-13711f796bce");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "7bc2c10b-f14c-4308-bebd-52351db560ae");
        }
    }
}
