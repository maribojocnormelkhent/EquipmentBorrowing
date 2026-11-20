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

public partial class EquipmentViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Equipment? selectedEquipment;

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
        }
        catch (Exception)
        {
            StatusMessage = "Could not load data from the database. Check the connection and try again.";
        }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select a piece of equipment.";
            return;
        }

        var request = new BorrowEquipmentRequest(
            SelectedStudent.Id, SelectedEquipment.Id, ExpectedReturnDate.DateTime);

        try
        {
            var result = await _executor.ExecuteAsync<BorrowEquipmentService, BorrowEquipmentResult>(
                s => s.ExecuteAsync(request));

            StatusMessage = result.Success
                ? $"Borrowed successfully. Borrowing #{result.Borrowing!.Id} created."
                : $"Could not borrow: {result.ErrorMessage}";
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