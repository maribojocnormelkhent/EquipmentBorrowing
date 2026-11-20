using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<Borrowing> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    public BorrowingsViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    public async Task LoadAsync()
    {
        try
        {
            var items = await _executor.ExecuteAsync<IBorrowingRepository, IReadOnlyList<Borrowing>>(
                r => r.GetActiveBorrowingsAsync());

            ActiveBorrowings.Clear();
            foreach (var borrowing in items)
                ActiveBorrowings.Add(borrowing);
        }
        catch (Exception)
        {
            StatusMessage = "Could not load borrowings from the database. Check the connection and try again.";
        }
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