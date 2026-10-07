using ConsignedCredit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Configurations
{
    public sealed class StateLoanRestrictionConfiguration
     : IEntityTypeConfiguration<StateLoanRestriction>
    {
        public void Configure(
            EntityTypeBuilder<StateLoanRestriction> builder)
        {
            builder.ToTable("StateLoanRestrictions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.State)
                .HasMaxLength(2)
                .IsRequired();

            builder.HasIndex(x => x.State)
                .IsUnique();

            builder.Property(x => x.MaximumAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();
        }
    }
}
