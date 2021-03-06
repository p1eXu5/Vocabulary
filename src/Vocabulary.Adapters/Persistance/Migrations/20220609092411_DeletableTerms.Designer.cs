// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vocabulary.Adapters.Persistance;

#nullable disable

namespace Vocabulary.BlazorServer.Migrations
{
    [DbContext(typeof(VocabularyDbContext))]
    [Migration("20220609092411_DeletableTerms")]
    partial class DeletableTerms
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("TermCategory", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TermId")
                        .HasColumnType("TEXT");

                    b.HasKey("CategoryId", "TermId");

                    b.HasIndex("TermId");

                    b.ToTable("TermCategory");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Category", (string)null);
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Link", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Href")
                        .IsRequired()
                        .HasMaxLength(2047)
                        .HasColumnType("TEXT");

                    b.Property<string>("ResourceDescription")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TermId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TermId");

                    b.ToTable("Link", "dbo");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Synonym", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TermId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TermId");

                    b.ToTable("Synonym", "dbo");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Term", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AdditionalName")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BOOLEAN")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int>("Sequence")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INT");

                    b.Property<ulong>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INT")
                        .HasDefaultValue(0ul);

                    b.Property<string>("ValidationRules")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasAlternateKey("Sequence");

                    b.ToTable("Term", "dbo");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.TermCategory", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TermId")
                        .HasColumnType("TEXT");

                    b.HasKey("CategoryId", "TermId");

                    b.HasIndex("TermId")
                        .HasDatabaseName("IX_TermCategory_TermId1");

                    b.ToTable("TermCategory", "dbo");
                });

            modelBuilder.Entity("TermCategory", b =>
                {
                    b.HasOne("Vocabulary.Adapters.Persistance.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_TermCategory_Category_CategoryId");

                    b.HasOne("Vocabulary.Adapters.Persistance.Models.Term", null)
                        .WithMany()
                        .HasForeignKey("TermId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired()
                        .HasConstraintName("FK_TermLink_Terms_TermId");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Link", b =>
                {
                    b.HasOne("Vocabulary.Adapters.Persistance.Models.Term", null)
                        .WithMany("Links")
                        .HasForeignKey("TermId");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Synonym", b =>
                {
                    b.HasOne("Vocabulary.Adapters.Persistance.Models.Term", null)
                        .WithMany("Synonyms")
                        .HasForeignKey("TermId");
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.TermCategory", b =>
                {
                    b.HasOne("Vocabulary.Adapters.Persistance.Models.Term", null)
                        .WithMany("TermCategories")
                        .HasForeignKey("TermId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Vocabulary.Adapters.Persistance.Models.Term", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("Synonyms");

                    b.Navigation("TermCategories");
                });
#pragma warning restore 612, 618
        }
    }
}
