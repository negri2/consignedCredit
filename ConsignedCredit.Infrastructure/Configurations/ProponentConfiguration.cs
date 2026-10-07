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
    public sealed class ProponentConfiguration
    : IEntityTypeConfiguration<Proponent>
    {
        public void Configure(EntityTypeBuilder<Proponent> builder)
        {
            builder.ToTable("Proponents");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Cpf)
                .HasMaxLength(11)
                .IsRequired();

            builder.HasIndex(x => x.Cpf)
                .IsUnique();

            builder.Property(x => x.InssNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.RetirementIncome)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.BirthDate)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(30)
                .IsRequired();

            builder.OwnsOne(x => x.Address, address =>
            {
                address.Property(x => x.Street)
                    .HasColumnName("AddressStreet")
                    .HasMaxLength(200)
                    .IsRequired();

                address.Property(x => x.Number)
                    .HasColumnName("AddressNumber")
                    .HasMaxLength(20);

                address.Property(x => x.City)
                    .HasColumnName("AddressCity")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.State)
                    .HasColumnName("AddressState")
                    .HasMaxLength(2)
                    .IsRequired();

                address.Property(x => x.ZipCode)
                    .HasColumnName("AddressZipCode")
                    .HasMaxLength(10);
            });
        }
    }
}
