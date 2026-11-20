namespace EquipmentBorrowing.Application.Services;

public class OperationResult<T>
{
    public bool Success { get; }
    public string Message { get; }
    public T? Value { get; }

    private OperationResult(bool success, string message, T? value)
    {
        Success = success;
        Message = message;
        Value = value;
    }

    public static OperationResult<T> Ok(T value, string message) => new(true, message, value);

    public static OperationResult<T> Fail(string message) => new(false, message, default);
}