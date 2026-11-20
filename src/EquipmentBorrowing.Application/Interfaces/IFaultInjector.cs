namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>Test seam: lets a demo force a failure inside a transaction.</summary>
public interface IFaultInjector
{
    void Check(string point);
}

public class NoFaultInjector : IFaultInjector
{
    public void Check(string point)
    {
    }
}