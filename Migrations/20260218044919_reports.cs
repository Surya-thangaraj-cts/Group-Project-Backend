using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserApi.Migrations
{
    /// <inheritdoc />
    public partial class reports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Reports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "Reports",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "GeneratedByUserId",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GrowthRate",
                table: "Reports",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Reports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Reports",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<int>(
                name: "TotalTransactions",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransactionStatus",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AccountId",
                table: "Reports",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_FromDate_ToDate",
                table: "Reports",
                columns: new[] { "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedByUserId",
                table: "Reports",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportType",
                table: "Reports",
                column: "ReportType");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Accounts_AccountId",
                table: "Reports",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Accounts_AccountId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AccountId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_FromDate_ToDate",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_GeneratedByUserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "GeneratedByUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "GrowthRate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TotalTransactions",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Reports");
        }
    }
}
