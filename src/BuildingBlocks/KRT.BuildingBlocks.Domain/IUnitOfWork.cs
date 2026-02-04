using System;
using System.Threading;
using System.Threading.Tasks;

namespace KRT.BuildingBlocks.Domain;

public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
