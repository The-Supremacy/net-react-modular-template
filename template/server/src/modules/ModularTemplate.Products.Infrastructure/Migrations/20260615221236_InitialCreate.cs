using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularTemplate.Products.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "products");

            migrationBuilder.CreateTable(
                name: "domain_event_records",
                schema: "products",
                columns: table => new
                {
                    DomainEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DomainEventName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayloadTypeName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    PayloadMetadata = table.Column<string>(type: "text", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TraceParent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TraceState = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TraceBaggage = table.Column<string>(type: "text", nullable: true),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_event_records", x => x.DomainEventId);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "products",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HandlerIdentity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.ModuleName, x.MessageId, x.HandlerIdentity });
                });

            migrationBuilder.CreateTable(
                name: "incoming_inbox_messages",
                schema: "products",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverModule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HandlerIdentity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    MessageKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MessageTypeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetModule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DurableOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TraceParent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TraceState = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TraceBaggage = table.Column<string>(type: "text", nullable: true),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartitionKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceTransportName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IngestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ClaimedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ClaimedUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incoming_inbox_messages", x => new { x.ReceiverModule, x.MessageId, x.HandlerIdentity });
                });

            migrationBuilder.CreateTable(
                name: "operation_states",
                schema: "products",
                columns: table => new
                {
                    DurableOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResultPayload = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    ModuleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MessageTypeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HandlerIdentity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_states", x => x.DurableOperationId);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "products",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MessageTypeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetModule = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DurableOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TraceParent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TraceState = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TraceBaggage = table.Column<string>(type: "text", nullable: true),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartitionKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StoredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DispatchedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ClaimedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ClaimedUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_records_ModuleName_CapturedAtUtc",
                schema: "products",
                table: "domain_event_records",
                columns: new[] { "ModuleName", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_records_ModuleName_DomainEventName",
                schema: "products",
                table: "domain_event_records",
                columns: new[] { "ModuleName", "DomainEventName" });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_ReceivedAtUtc",
                schema: "products",
                table: "inbox_messages",
                column: "ReceivedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_DurableOperationId",
                schema: "products",
                table: "incoming_inbox_messages",
                column: "DurableOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_MessageTypeName",
                schema: "products",
                table: "incoming_inbox_messages",
                column: "MessageTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_ReceiverModule_Status_NextAttemptAt~",
                schema: "products",
                table: "incoming_inbox_messages",
                columns: new[] { "ReceiverModule", "Status", "NextAttemptAtUtc", "IngestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_SourceTransportName",
                schema: "products",
                table: "incoming_inbox_messages",
                column: "SourceTransportName");

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_Status_ClaimedUntilUtc",
                schema: "products",
                table: "incoming_inbox_messages",
                columns: new[] { "Status", "ClaimedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_incoming_inbox_messages_Status_NextAttemptAtUtc_IngestedAtU~",
                schema: "products",
                table: "incoming_inbox_messages",
                columns: new[] { "Status", "NextAttemptAtUtc", "IngestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_operation_states_Status",
                schema: "products",
                table: "operation_states",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_operation_states_UpdatedAtUtc",
                schema: "products",
                table: "operation_states",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_DurableOperationId",
                schema: "products",
                table: "outbox_messages",
                column: "DurableOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_MessageTypeName",
                schema: "products",
                table: "outbox_messages",
                column: "MessageTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_ClaimedUntilUtc",
                schema: "products",
                table: "outbox_messages",
                columns: new[] { "Status", "ClaimedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_NextAttemptAtUtc_StoredAtUtc",
                schema: "products",
                table: "outbox_messages",
                columns: new[] { "Status", "NextAttemptAtUtc", "StoredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_products_CreatedAtUtc",
                schema: "products",
                table: "products",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_products_Name",
                schema: "products",
                table: "products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_event_records",
                schema: "products");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "products");

            migrationBuilder.DropTable(
                name: "incoming_inbox_messages",
                schema: "products");

            migrationBuilder.DropTable(
                name: "operation_states",
                schema: "products");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "products");

            migrationBuilder.DropTable(
                name: "products",
                schema: "products");
        }
    }
}
