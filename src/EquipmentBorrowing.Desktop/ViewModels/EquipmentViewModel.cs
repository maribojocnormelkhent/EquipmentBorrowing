using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private DateTimeOffset expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string? statusMessage;

    public EquipmentViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    public async Task LoadAsync()
    {
        try
        {
            var items = await _executor.ExecuteAsync<IEquipmentRepository, IReadOnlyList<Equipment>>(
                r => r.GetAllAsync());
            var students = await _executor.ExecuteAsync<IStudentRepository, IReadOnlyList<Student>>(
                r => r.GetAllAsync());

            EquipmentItems.Clear();
            foreach (var item in items)
                EquipmentItems.Add(item);

            Students.Clear();
            foreach (var student in students)
                Students.Add(student);

            if (items.Count == 0)
                StatusMessage = "No equipment registered yet.";
        }
        catch (Exception)
        {
            StatusMessage = "Could not load data from the database. Check the connection and try again.";
        }
    }

    // UC-03: exactly one selected item.
    [RelayCommand]
    private async Task BorrowAsync(IList? selectedItems)
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        var items = selectedItems?.Cast<Equipment>().ToList() ?? new List<Equipment>();
        if (items.Count != 1)
        {
            StatusMessage = "Select exactly one item to borrow, or use 'Check Out Selected' for two or more.";
            return;
        }

        var request = new BorrowEquipmentRequest(
            SelectedStudent.Id, items[0].Id, ExpectedReturnDate.DateTime);

        await RunAsync(async () =>
        {
            var result = await _executor.ExecuteAsync<BorrowEquipmentService, BorrowEquipmentResult>(
                s => s.ExecuteAsync(request));

            return result.Success
                ? $"Borrowed successfully. Borrowing #{result.Borrowing!.Id} created."
                : $"Could not borrow: {result.ErrorMessage}";
        });
    }

    // UC-06: two or more selected items, committed together or not at all.
    [RelayCommand]
    private async Task CheckOutSelectedAsync(IList? selectedItems)
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        var ids = selectedItems?.Cast<Equipment>().Select(e => e.Id).ToList() ?? new List<int>();
        var request = new CheckOutMultipleRequest(SelectedStudent.Id, ids, ExpectedReturnDate.DateTime);

        await RunAsync(async () =>
        {
            var result = await _executor.ExecuteAsync<CheckOutMultipleService, OperationResult<IReadOnlyList<Borrowing>>>(
                s => s.ExecuteAsync(request));

            return result.Success ? result.Message : $"Could not check out: {result.Message}";
        });
    }

    private async Task RunAsync(Func<Task<string>> operation)
    {
        try
        {
            StatusMessage = await operation();
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