using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parqueadero.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarParqueaderosPorPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoParqueadero",
                table: "RegistrosAcceso",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ArenaUSCAbierta",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CupoArenaUSC",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoDocentes",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoEgresados",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CupoEstudiantes",
                table: "ConfiguracionParqueadero",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorreoPersonal",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorreoUniversidad",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ConfiguracionParqueadero",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ArenaUSCAbierta", "CupoArenaUSC", "CupoDocentes", "CupoEgresados", "CupoEstudiantes" },
                values: new object[] { false, 5, 15, 10, 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoParqueadero",
                table: "RegistrosAcceso");

            migrationBuilder.DropColumn(
                name: "ArenaUSCAbierta",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoArenaUSC",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoDocentes",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoEgresados",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CupoEstudiantes",
                table: "ConfiguracionParqueadero");

            migrationBuilder.DropColumn(
                name: "CorreoPersonal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CorreoUniversidad",
                table: "AspNetUsers");
        }
    }
}
