using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.Adapters.Persistance.EntityTypeConfigurations
{
    public class TermConfiguration : IEntityTypeConfiguration<Term>
    {
        public void Configure(EntityTypeBuilder<Term> builder)
        {
            builder.ToTable("Term", "dbo");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Sequence)
                .HasColumnType("INT")
                .IsRequired()
                .ValueGeneratedOnAdd(); // does not work on sqlite

            builder.Property(t => t.Timestamp)
                .IsRequired()
                .HasColumnType("INT");

            builder.HasAlternateKey(t => t.Sequence);

            builder.HasMany(t => t.Synonyms).WithOne();
            builder
                .HasMany(t => t.Links)
                .WithOne();

            builder
                .HasMany(t => t.Categories)
                .WithMany(l => l.Terms)
                .UsingEntity<Dictionary<string, object>>(
                    "TermCategory",
                    j => j
                        .HasOne<Category>()
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK_TermCategory_Category_CategoryId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Term>()
                        .WithMany()
                        .HasForeignKey("TermId")
                        .HasConstraintName("FK_TermLink_Terms_TermId")
                        .OnDelete(DeleteBehavior.ClientCascade));
        }
    }
}
