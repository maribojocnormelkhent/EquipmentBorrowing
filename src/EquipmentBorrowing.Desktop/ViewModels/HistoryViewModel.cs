using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;
    private int _studentRequestId;
    private int _equipmentRequestId;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<BorrowingDetail> StudentRecords { get; } = new();
    public ObservableCollection<BorrowingDetail> EquipmentRecords { get; } = new();

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;

    [ObservableProperty] private bool isStudentLoading;
    [ObservableProperty] private string? studentMessage;
    [ObservableProperty] private bool studentMessageIsError;

    [ObservableProperty] private bool isEquipmentLoading;
    [ObservableProperty] private string? equipmentMessage;
    [ObservableProperty] private bool equipmentMessageIsError;

    public HistoryViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    partial void OnSelectedStudentChanged(Student? value) => _ = LoadStudentHistoryAsync(value);

    partial void OnSelectedEquipmentChanged(Equipment? value) => _ = LoadEquipmentHistoryAsync(value);

    public async Task LoadAsync()
    {
        var studentId = SelectedStudent?.Id;
        var equipmentId = SelectedEquipment?.Id;

        try
        {
            var students = await _executor.ExecuteAsync<StudentManagementService, IReadOnlyList<Student>>(
                s => s.SearchAsync(null));
            var equipment = await _executor.ExecuteAsync<EquipmentManagementService, IReadOnlyList<Equipment>>(
                s => s.SearchAsync(null, false));

            Students.Clear();
            foreach (var student in students)
                Students.Add(student);

            EquipmentItems.Clear();
            foreach (var item in equipment)
                EquipmentItems.Add(item);

            // Re-selecting refreshes the history of the previously selected subject.
            SelectedStudent = Students.FirstOrDefault(s => s.Id == studentId);
            SelectedEquipment = EquipmentItems.FirstOrDefault(e => e.Id == equipmentId);
        }
        catch (Exception)
        {
            StudentMessage = "Could not load students. Check the database connection and press Reload.";
            StudentMessageIsError = true;
            EquipmentMessage = "Could not load equipment. Check the database connection and press Reload.";
            EquipmentMessageIsError = true;
        }
    }

    [RelayCommand]
    private Task ReloadAsync() => LoadAsync();

    private async Task LoadStudentHistoryAsync(Student? student)
    {
        var request = ++_studentRequestId;
        StudentRecords.Clear();
        StudentMessage = null;
        StudentMessageIsError = false;

        if (student is null)
        {
            IsStudentLoading = false;
            return;
        }

        IsStudentLoading = true;
        try
        {
            var result = await _executor.ExecuteAsync<BorrowingQueryService, OperationResult<IReadOnlyList<BorrowingDetail>>>(
                s => s.GetStudentHistoryAsync(student.Id));

            if (request != _studentRequestId)
                return;

            if (!result.Success)
            {
                StudentMessage = result.Message;
                StudentMessageIsError = true;
                return;
            }

            foreach (var record in result.Value!)
                StudentRecords.Add(record);

            StudentMessage = result.Message;
        }
        catch (Exception)
        {
            if (request != _studentRequestId)
                return;

            StudentMessage = "Could not load this student's history. Check the database connection and try again.";
            StudentMessageIsError = true;
        }
        finally
        {
            if (request == _studentRequestId)
                IsStudentLoading = false;
        }
    }

    private async Task LoadEquipmentHistoryAsync(Equipment? item)
    {
        var request = ++_equipmentRequestId;
        EquipmentRecords.Clear();
        EquipmentMessage = null;
        EquipmentMessageIsError = false;

        if (item is null)
        {
            IsEquipmentLoading = false;
            return;
        }

        IsEquipmentLoading = true;
        try
        {
            var result = await _executor.ExecuteAsync<BorrowingQueryService, OperationResult<IReadOnlyList<BorrowingDetail>>>(
                s => s.GetEquipmentHistoryAsync(item.Id));

            if (request != _equipmentRequestId)
                return;

            if (!result.Success)
            {
                EquipmentMessage = result.Message;
                EquipmentMessageIsError = true;
                return;
            }

            foreach (var record in result.Value!)
                EquipmentRecords.Add(record);

            EquipmentMessage = result.Message;
        }
        catch (Exception)
        {
            if (request != _equipmentRequestId)
                return;

            EquipmentMessage = "Could not load this item's history. Check the database connection and try again.";
            EquipmentMessageIsError = true;
        }
        finally
        {
            if (request == _equipmentRequestId)
                IsEquipmentLoading = false;
        }
    }
}