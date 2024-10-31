using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trawler.Database.Migration
{
    /// <inheritdoc />
    public partial class InitialMigration : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "crawl_targets",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CrawlType = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetScreenName = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    WorkaroundPostId = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crawl_targets", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "twitter_account_crawl_results",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FollowerCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    FollowingCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    PostCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    CrawlTargetId = table.Column<uint>(type: "int unsigned", nullable: false),
                    CrawlDoneAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_account_crawl_results", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "twitter_post_crawl_results",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ViewCount = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    BookmarkCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    LikesCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    RetweetsCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    QuotesCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    RepliesCount = table.Column<uint>(type: "int unsigned", nullable: false),
                    CrawlTargetId = table.Column<uint>(type: "int unsigned", nullable: false),
                    CrawlDoneAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_post_crawl_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_post_crawl_results_crawl_targets_CrawlTargetId",
                        column: x => x.CrawlTargetId,
                        principalTable: "crawl_targets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_post_crawl_results_CrawlTargetId",
                table: "twitter_post_crawl_results",
                column: "CrawlTargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_account_crawl_results");

            migrationBuilder.DropTable(
                name: "twitter_post_crawl_results");

            migrationBuilder.DropTable(
                name: "crawl_targets");
        }
    }
}
