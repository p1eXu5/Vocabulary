using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.Adapters.Persistance.EntityTypeConfigurations
{
    public class TermCategoryConfiguration : IEntityTypeConfiguration<TermCategory>
    {
        public void Configure(EntityTypeBuilder<TermCategory> builder)
        {
            builder.ToTable("TermCategory", "dbo");
            builder.HasKey(tc => new { tc.CategoryId, tc.TermId });
        }
    }
}
