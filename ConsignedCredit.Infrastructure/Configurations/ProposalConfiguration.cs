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
    public sealed class ProposalConfiguration
    : IEntityTypeConfiguration<Proposal>
    {
        public void Configure(EntityTypeBuilder<Proposal> builder)
        {
            builder.ToTable("Proposals");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgentId)
                .IsRequired();

            builder.Property(x => x.StoreId)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Proponent)
                .WithMany()
                .HasForeignKey("ProponentId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(x => x.Simulation, simulation =>
            {
                simulation.Property(x => x.RequestedAmount)
                    .HasColumnName("RequestedAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                simulation.Property(x => x.Installments)
                    .HasColumnName("Installments")
                    .IsRequired();

                simulation.Property(x => x.InstallmentAmount)
                    .HasColumnName("InstallmentAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();
            });
        }
    }
}
