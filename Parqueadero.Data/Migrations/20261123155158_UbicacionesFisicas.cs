using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parqueadero.Data.Migrations
{
    /// <inheritdoc />
    public partial class UbicacionesFisicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CupoAtrasCanchas",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoBloque6",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoBloque8",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoPlazoleta",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ConfiguracionParqueadero",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CupoAtrasCanchas", "CupoBloque6", "CupoBloque8", "CupoPlazoleta" },
                values: new object[] { 15, 20, 15, 10 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CupoAtrasCanchas",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoBloque6",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoBloque8",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoPlazoleta",
                table: "ConfiguracionParqueadero");
        }
    }
}
