using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventSourcR.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventNumber = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventId = table.Column<Guid>(nullable: false),
                    EventType = table.Column<string>(nullable: true),
                    AggregateId = table.Column<Guid>(nullable: false),
                    AggregateType = table.Column<string>(nullable: true),
                    AggregateVersion = table.Column<long>(nullable: false),
                    SerializedData = table.Column<string>(nullable: true),
                    SerializedMetaData = table.Column<string>(nullable: true),
                    Recorded = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventNumber);
                    table.UniqueConstraint("AK_Events_EventId", x => x.EventId);
                    table.UniqueConstraint("AK_Events_AggregateId_AggregateVersion", x => new { x.AggregateId, x.AggregateVersion });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AggregateId",
                table: "Events",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_AggregateType",
                table: "Events",
                column: "AggregateType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType",
                table: "Events",
                column: "EventType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
