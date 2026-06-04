using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class MakeCandidateNullableInResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_Candidates_CandidateId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateId",
                table: "Resumes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes",
                column: "CandidateId",
                unique: true,
                filter: "[CandidateId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_Candidates_CandidateId",
                table: "Resumes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_Candidates_CandidateId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateId",
                table: "Resumes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes",
                column: "CandidateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_Candidates_CandidateId",
                table: "Resumes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
