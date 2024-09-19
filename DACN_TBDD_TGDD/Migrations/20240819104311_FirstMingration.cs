using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN_TBDD_TGDD.Migrations
{
    /// <inheritdoc />
    public partial class FirstMingration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OrderDetails",
                newName: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "OrderDetails",
                newName: "UserId");
        }
    }
}
