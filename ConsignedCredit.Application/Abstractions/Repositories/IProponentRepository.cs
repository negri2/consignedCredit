using ConsignedCredit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Repositories
{
    public interface IProponentRepository
    {
        Task<Proponent?> GetByCpfAsync(
            string cpf,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Proponent proponent,
            CancellationToken cancellationToken = default);
    }
}
