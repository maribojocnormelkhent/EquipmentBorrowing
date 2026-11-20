using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default) => operation(cancellationToken);
}