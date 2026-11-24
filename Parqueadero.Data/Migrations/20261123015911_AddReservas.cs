using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parqueadero.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservasParqueadero",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoTicket = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    TarjetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHoraDeseada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracionHoras = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasParqueadero", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservasParqueadero_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservasParqueadero_Tarjetas_TarjetaId",
                        column: x => x.TarjetaId,
                        principalTable: "Tarjetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservasParqueadero_CodigoTicket",
                table: "ReservasParqueadero",
                column: "CodigoTicket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservasParqueadero_TarjetaId",
                table: "ReservasParqueadero",
                column: "TarjetaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasParqueadero_UsuarioId_FechaHoraDeseada_Estado",
                table: "ReservasParqueadero",
                columns: new[] { "UsuarioId", "FechaHoraDeseada", "Estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservasParqueadero");
        }
    }
}
