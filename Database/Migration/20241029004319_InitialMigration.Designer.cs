﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trawler.Database;

#nullable disable

namespace Trawler.Database.Migration
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20241029004319_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_general_ci")
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Trawler.Database.Model.CrawlResult", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("Id"));

                    b.Property<uint>("CrawlTargetId")
                        .HasColumnType("int unsigned");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<uint>("FollowerCount")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("FollowingCount")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("PostCount")
                        .HasColumnType("int unsigned");

                    b.HasKey("Id");

                    b.HasIndex("CrawlTargetId");

                    b.ToTable("crawl_results", (string)null);
                });

            modelBuilder.Entity("Trawler.Database.Model.CrawlTarget", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("Id"));

                    b.Property<string>("CrawlType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TargetId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("WorkaroundPostId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("crawl_targets", (string)null);
                });

            modelBuilder.Entity("Trawler.Database.Model.CrawlResult", b =>
                {
                    b.HasOne("Trawler.Database.Model.CrawlTarget", "CrawlTarget")
                        .WithMany("CrawlResults")
                        .HasForeignKey("CrawlTargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CrawlTarget");
                });

            modelBuilder.Entity("Trawler.Database.Model.CrawlTarget", b =>
                {
                    b.Navigation("CrawlResults");
                });
#pragma warning restore 612, 618
        }
    }
}
