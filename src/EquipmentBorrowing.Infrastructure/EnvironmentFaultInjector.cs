using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Infrastructure;

/// <summary>
/// Demo-only: throws at a named point when the environment variable
/// EQUIPMENT_BORROWING_FAULT_POINT equals that point. Does nothing otherwise.
/// </summary>
public class EnvironmentFaultInjector : IFaultInjector
{
    public void Check(string point)
    {
        var configured = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_FAULT_POINT");
        if (string.Equals(configured, point, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Injected test failure at '{point}'.");
    }
}