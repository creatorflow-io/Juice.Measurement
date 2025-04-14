using System;
using Juice.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Juice.Measurement.Stores.EF.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        private readonly string _schema;

        public Init()
        {
            _schema = "Measurement";
        }

        public Init(ISchemaDbContext schema)
        {
            _schema = schema.Schema;
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (_schema != null)
            {
                migrationBuilder.EnsureSchema(name: _schema);
            }

            migrationBuilder.CreateTable(
                name: "TimeRecords",
                schema: _schema,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    StartedTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ElapsedTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ScopeId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TraceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSummaries",
                schema: _schema,
                columns: table => new
                {
                    TraceId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RootScopeId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    RecordedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSummaries", x => x.TraceId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_Name",
                schema: _schema,
                table: "TimeRecords",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_ScopeId",
                schema: _schema,
                table: "TimeRecords",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_TraceId",
                schema: _schema,
                table: "TimeRecords",
                column: "TraceId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_RecordedDate",
                schema: _schema,
                table: "TimeRecords",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSummaries_Name",
                schema: _schema,
                table: "TimeSummaries",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSummaries_RecordedDate",
                schema: _schema,
                table: "TimeSummaries",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSummaries_RootScopeId",
                schema: _schema,
                table: "TimeSummaries",
                column: "RootScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeRecords",
                schema: _schema);

            migrationBuilder.DropTable(
                name: "TimeSummaries",
                schema: _schema);
        }
    }
}
