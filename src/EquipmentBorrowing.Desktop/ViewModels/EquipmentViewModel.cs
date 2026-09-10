using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>
/// Presentation state for the Equipment area. Collects input and calls
/// BorrowEquipmentService — it does NOT re-implement any borrowing rule.
/// </summary>
public partial class EquipmentViewModel : ObservableObject
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

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

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        IStudentRepository studentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentRepository = equipmentRepository;
        _studentRepository = studentRepository;
        _borrowEquipmentService = borrowEquipmentService;
    }

    public async Task LoadAsync()
    {
        EquipmentItems.Clear();
        foreach (var item in await _equipmentRepository.GetAllAsync())
            EquipmentItems.Add(item);

        Students.Clear();
        foreach (var student in await _studentRepository.GetAllAsync())
            Students.Add(student);
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        // --- Presentation validation: is the input even usable? ---
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

        // --- Business decision: delegated entirely to the service. ---
        var result = await _borrowEquipmentService.ExecuteAsync(
            new BorrowEquipmentRequest(SelectedStudent.Id, SelectedEquipment.Id, ExpectedReturnDate.DateTime));

        StatusMessage = result.Success
            ? $"Borrowed successfully. Borrowing #{result.Borrowing!.Id} created."
            : $"Could not borrow: {result.ErrorMessage}";

        await LoadAsync(); // refresh the list so availability updates on screen
    }
}