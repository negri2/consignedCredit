using ConsignedCredit.Application.Proposals.Create;
using ConsignedCredit.Application.Proposals.Process;
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

            return services;
        }
    }
}
