using ConsignedCredit.Application.Abstractions.Messaging;
using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Infrastructure.ExternalServices;
using ConsignedCredit.Infrastructure.Messaging.Outbox;
using ConsignedCredit.Infrastructure.Messaging.RabbitMq;
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
            services.AddScoped<IStateLoanRestrictionRepository, StateLoanRestrictionRepository>();

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ConsignedCreditDbContext>());

            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

            services.AddScoped<IOutbox, Outbox>();

            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IFraudCheckService, FraudCheckService>();
            services.AddScoped<ISimulationValidationService, SimulationValidationService>();
            services.AddScoped<IRiskAnalysisService, RiskAnalysisService>();
            services.AddScoped<IInssRegistrationService, InssRegistrationService>();
            services.AddScoped<IContractGenerationService, ContractGenerationService>();
            services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }

        public static IServiceCollection AddOutboxProcessor(
            this IServiceCollection services)
        {
            services.AddHostedService<OutboxProcessor>();

            return services;
        }
    }
}
