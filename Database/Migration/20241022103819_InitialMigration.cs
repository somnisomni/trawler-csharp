using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trawler.Database.Migration {
  /// <inheritdoc />
  public partial class InitialMigration : Microsoft.EntityFrameworkCore.Migrations.Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");

      var table = migrationBuilder.CreateTable(
        name: "crawl_targets",
        columns: table => new {
          Id = table.Column<uint>(nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
          CrawlType = table.Column<string>(type: "TINYTEXT", nullable: false),
          TargetId = table.Column<string>(type: "TEXT", nullable: false),
          WorkaroundPostId = table.Column<string>(type: "TEXT", nullable: true)
        },
        constraints: table => {
          table.PrimaryKey("PK_crawl_targets", x => x.Id);
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable("crawl_targets");
    }
  }
}