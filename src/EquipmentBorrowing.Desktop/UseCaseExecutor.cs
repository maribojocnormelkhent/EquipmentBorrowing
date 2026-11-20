using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

/// <summary>
/// Runs one UI operation inside its own DI scope, so each operation gets a fresh
/// DbContext (short-lived, never shared across overlapping async operations).
/// </summary>
public interface IUseCaseExecutor
{
    Task<TResult> ExecuteAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull;
}

public class UseCaseExecutor : IUseCaseExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UseCaseExecutor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<TResult> ExecuteAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await action(service);
    }
}