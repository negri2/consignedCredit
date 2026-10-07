using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.Enums
{
    public enum ProposalProcessingStep
    {
        None = 0,
        SimulationValidation = 1,
        RiskAnalysis = 2,
        InssRegistration = 3,
        ContractGeneration = 4,
        DigitalSignature = 5,
        Payment = 6,
        Completed = 7
    }
}
