using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public record RegisterEquipmentRequest(string? Identifier, string? Name, string? Description);

public record UpdateEquipmentRequest(int EquipmentId, string? Identifier, string? Name, string? Description);

public class EquipmentManagementService
{
    private const int MaxIdentifierLength = 50;
    private const int MaxNameLength = 150;
    private const int MaxDescriptionLength = 500;

    private readonly IEquipmentRepository _equipment;
    private readonly IUnitOfWork _unitOfWork;

    public EquipmentManagementService(IEquipmentRepository equipment, IUnitOfWork unitOfWork)
    {
        _equipment = equipment;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Equipment>> SearchAsync(
        string? searchText, bool availableOnly, CancellationToken cancellationToken = default)
        => _equipment.SearchAsync(searchText, availableOnly, cancellationToken);

    // UC-15
    public async Task<OperationResult<Equipment>> RegisterAsync(
        RegisterEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var error = Validate(request.Identifier, request.Name, request.Description,
            out var identifier, out var name, out var description);
        if (error is not null)
            return OperationResult<Equipment>.Fail(error);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                if (await _equipment.GetByIdentifierAsync(identifier, token) is not null)
                    return OperationResult<Equipment>.Fail(DuplicateMessage(identifier));

                // New equipment is available with no active borrowing (BR-06).
                var item = new Equipment(0, name, identifier, isAvailable: true, description: description);
                await _equipment.AddAsync(item, token);
                return OperationResult<Equipment>.Ok(item, $"Equipment '{item.Name}' registered as available.");
            }, cancellationToken);
        }
        catch (DuplicateValueException)
        {
            return OperationResult<Equipment>.Fail(DuplicateMessage(identifier));
        }
    }

    // UC-16: only descriptive fields can change; availability is never edited here (BR-07).
    public async Task<OperationResult<Equipment>> UpdateAsync(
        UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        var error = Validate(request.Identifier, request.Name, request.Description,
            out var identifier, out var name, out var description);
        if (error is not null)
            return OperationResult<Equipment>.Fail(error);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                var item = await _equipment.GetByIdAsync(request.EquipmentId, token);
                if (item is null)
                    return OperationResult<Equipment>.Fail(
                        "This equipment no longer exists. Reload the list and select again.");

                var other = await _equipment.GetByIdentifierAsync(identifier, token);
                if (other is not null && other.Id != item.Id)
                    return OperationResult<Equipment>.Fail(DuplicateMessage(identifier));

                item.UpdateDetails(identifier, name, description);
                await _equipment.UpdateAsync(item, token);
                return OperationResult<Equipment>.Ok(item, $"Equipment '{item.Name}' updated.");
            }, cancellationToken);
        }
        catch (DuplicateValueException)
        {
            return OperationResult<Equipment>.Fail(DuplicateMessage(identifier));
        }
    }

    private static string DuplicateMessage(string identifier) =>
        $"Equipment with identifier '{identifier}' already exists.";

    private static string? Validate(
        string? rawIdentifier, string? rawName, string? rawDescription,
        out string identifier, out string name, out string? description)
    {
        identifier = rawIdentifier?.Trim() ?? string.Empty;
        name = rawName?.Trim() ?? string.Empty;
        description = string.IsNullOrWhiteSpace(rawDescription) ? null : rawDescription.Trim();

        if (identifier.Length == 0) return "Equipment identifier is required.";
        if (name.Length == 0) return "Equipment name is required.";
        if (identifier.Length > MaxIdentifierLength) return $"Identifier must be at most {MaxIdentifierLength} characters.";
        if (name.Length > MaxNameLength) return $"Name must be at most {MaxNameLength} characters.";
        if (description is not null && description.Length > MaxDescriptionLength)
            return $"Description must be at most {MaxDescriptionLength} characters.";
        return null;
    }
}