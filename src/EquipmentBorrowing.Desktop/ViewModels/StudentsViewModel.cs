using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty] private string? searchText;
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private string? editStudentNumber;
    [ObservableProperty] private string? editName;
    [ObservableProperty] private string? editContact;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    public StudentsViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is null)
            return;

        EditStudentNumber = value.StudentNumber;
        EditName = value.Name;
        EditContact = value.Contact;
    }

    public async Task LoadAsync(int? reselectId = null)
    {
        IsBusy = true;
        try
        {
            var items = await _executor.ExecuteAsync<StudentManagementService, IReadOnlyList<Student>>(
                s => s.SearchAsync(SearchText));

            Students.Clear();
            foreach (var item in items)
                Students.Add(item);

            if (reselectId.HasValue)
                SelectedStudent = Students.FirstOrDefault(s => s.Id == reselectId.Value);

            if (items.Count == 0)
                StatusMessage = string.IsNullOrWhiteSpace(SearchText)
                    ? "No students registered yet."
                    : "No students match your search.";
        }
        catch (Exception)
        {
            StatusMessage = "Could not load students from the database. Check the connection and try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        StatusMessage = null;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = null;
        StatusMessage = null;
        await LoadAsync();
    }

    [RelayCommand]
    private void NewStudent()
    {
        SelectedStudent = null;
        EditStudentNumber = null;
        EditName = null;
        EditContact = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task RegisterAsync() =>
        RunAsync(s => s.RegisterAsync(new RegisterStudentRequest(EditStudentNumber, EditName, EditContact)));

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student from the list to edit first.";
            return;
        }

        var id = SelectedStudent.Id;
        await RunAsync(s => s.UpdateAsync(
            new UpdateStudentRequest(id, EditStudentNumber, EditName, EditContact)));
    }

    [RelayCommand]
    private Task PermitAsync() => ChangeEligibilityAsync(true);

    [RelayCommand]
    private Task RestrictAsync() => ChangeEligibilityAsync(false);

    private async Task ChangeEligibilityAsync(bool allowed)
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student from the list first.";
            return;
        }

        var id = SelectedStudent.Id;
        await RunAsync(s => s.SetEligibilityAsync(id, allowed));
    }

    private async Task RunAsync(Func<StudentManagementService, Task<OperationResult<Student>>> operation)
    {
        IsBusy = true;
        try
        {
            var result = await _executor.ExecuteAsync<StudentManagementService, OperationResult<Student>>(operation);
            StatusMessage = result.Message;

            if (result.Success)
                await LoadAsync(result.Value!.Id);
        }
        catch (CommitOutcomeUnknownException ex)
        {
            StatusMessage = ex.Message;
            await LoadAsync();
        }
        catch (PersistenceException ex)
        {
            StatusMessage = ex.Message;
            await LoadAsync();
        }
        catch (Exception)
        {
            StatusMessage = "Unexpected error. Nothing was confirmed; reload and check before retrying.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}