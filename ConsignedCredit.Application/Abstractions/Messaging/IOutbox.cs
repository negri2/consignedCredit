using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Messaging
{
    public interface IOutbox
    {
        Task AddAsync<T>(
            T message,
            CancellationToken cancellationToken = default);
    }
}
