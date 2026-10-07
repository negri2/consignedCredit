using ConsignedCredit.Application.Abstractions.Messaging;
using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Infrastructure.ExternalServices;
using ConsignedCredit.Infrastructure.Messaging.Outbox;
using ConsignedCredit.Infrastructure.Persistence;
using ConsignedCredit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ConsignedCreditDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Database")));

            services.AddScoped<IProposalRepository, ProposalRepository>();
            services.AddScoped<IProponentRepository, ProponentRepository>();

            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IFraudCheckService, FraudCheckService>();

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<ConsignedCreditDbContext>());

            services.AddScoped<IOutbox, Outbox>();

            return services;
        }
    }
}
