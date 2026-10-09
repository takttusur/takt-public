using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Takt.People.API.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FathersName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Alias = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BirthYear = table.Column<int>(type: "integer", nullable: true),
                    BirthMonth = table.Column<int>(type: "integer", nullable: true),
                    BirthDay = table.Column<int>(type: "integer", nullable: true),
                    IsAnonymized = table.Column<bool>(type: "boolean", nullable: false),
                    AnonymizationKeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AnonymizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalDataConsents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedFromIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDataConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalDataConsents_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_People_Alias",
                table: "People",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_AnonymizationKeyHash",
                table: "People",
                column: "AnonymizationKeyHash");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDataConsents_PersonId",
                table: "PersonalDataConsents",
                column: "PersonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalDataConsents");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
