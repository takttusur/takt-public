using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Takt.People.API.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClubStructureEnumRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubMemberships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<long>(type: "bigint", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FinishDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMemberships", x => x.Id);
                    table.CheckConstraint("CK_ClubMemberships_FinishDate", "\"FinishDate\" IS NULL OR \"FinishDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_ClubMemberships_RoleId", "\"RoleId\" IN (101, 102, 201, 202, 301)");
                    table.ForeignKey(
                        name: "FK_ClubMemberships_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubMemberships_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Name",
                table: "Branches",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_BranchId",
                table: "ClubMemberships",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_PersonId",
                table: "ClubMemberships",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_PersonId_RoleId_BranchId_StartDate",
                table: "ClubMemberships",
                columns: new[] { "PersonId", "RoleId", "BranchId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_RoleId",
                table: "ClubMemberships",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubMemberships");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
