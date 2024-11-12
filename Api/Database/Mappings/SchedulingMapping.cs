using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Mappings
{
    public class SchedulingMapping : IEntityTypeConfiguration<Scheduling>
    {
        public void Configure(EntityTypeBuilder<Scheduling> builder)
        {
            builder.ToTable(nameof(Scheduling));
            builder.HasKey(x => x.MessageNumber);
        }
    }
}
