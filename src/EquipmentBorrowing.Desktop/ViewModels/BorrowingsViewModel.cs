using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<BorrowingDetail> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private BorrowingDetail? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private string? emptyMessage;

    [ObservableProperty]
    private bool isLoading;

    public BorrowingsViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        EmptyMessage = null;
        try
        {
            var items = await _executor.ExecuteAsync<BorrowingQueryService, IReadOnlyList<BorrowingDetail>>(
                s => s.GetActiveAsync());

            ActiveBorrowings.Clear();
            foreach (var item in items)
                ActiveBorrowings.Add(item);

            EmptyMessage = items.Count == 0 ? "No active borrowings." : null;
        }
        catch (Exception)
        {
            StatusMessage = "Could not load borrowings from the database. Check the connection and try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        StatusMessage = null;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select a borrowing to return.";
            return;
        }

        var request = new ReturnEquipmentRequest(SelectedBorrowing.Id);

        try
        {
            var result = await _executor.ExecuteAsync<ReturnEquipmentService, ReturnEquipmentResult>(
                s => s.ExecuteAsync(request));

            StatusMessage = result.Success
                ? $"Borrowing #{result.Borrowing!.Id} returned successfully."
                : $"Could not return: {result.ErrorMessage}";
        }
        catch (CommitOutcomeUnknownException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (PersistenceException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception)
        {
            StatusMessage = "Unexpected error. Nothing was confirmed; reload and check before retrying.";
        }

        await LoadAsync();
    }
}