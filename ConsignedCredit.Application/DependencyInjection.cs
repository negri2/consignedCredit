using ConsignedCredit.Application.Proposals.Create;
using ConsignedCredit.Application.Proposals.Get;
using ConsignedCredit.Application.Proposals.Process;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<CreateProposalUseCase>();
            services.AddScoped<ProcessProposalUseCase>();
            services.AddScoped<GetProposalUseCase>();
            services.AddScoped<IValidator<CreateProposalRequest>, CreateProposalValidator>();

            return services;
        }
    }
}
