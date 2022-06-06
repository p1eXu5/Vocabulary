using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.Adapters.Persistance.EntityTypeConfigurations
{
    public class SynonymConfiguration : IEntityTypeConfiguration<Synonym>
    {
        public void Configure(EntityTypeBuilder<Synonym> builder)
        {
            builder.ToTable("Synonym", "dbo");
            builder.Property<int>("Id").ValueGeneratedOnAdd();
            builder.HasKey("Id");
        }
    }
}
