using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Persistence
{
    public sealed class ConsignedCreditDbContext
    : DbContext, IUnitOfWork
    {
        public ConsignedCreditDbContext(
            DbContextOptions<ConsignedCreditDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proposal> Proposals => Set<Proposal>();
        public DbSet<Proponent> Proponents => Set<Proponent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ConsignedCreditDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        async Task IUnitOfWork.SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await SaveChangesAsync(cancellationToken);
        }
    }
}
