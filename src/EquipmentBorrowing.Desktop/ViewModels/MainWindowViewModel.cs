using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;
    private readonly StudentsViewModel _studentsViewModel;
    private readonly EquipmentManagementViewModel _equipmentManagementViewModel;

    [ObservableProperty]
    private object? currentViewModel;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel,
        StudentsViewModel studentsViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        _studentsViewModel = studentsViewModel;
        _equipmentManagementViewModel = equipmentManagementViewModel;
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private async Task ShowEquipmentAsync()
    {
        CurrentViewModel = _equipmentViewModel;
        await _equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        CurrentViewModel = _borrowingsViewModel;
        await _borrowingsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowStudentsAsync()
    {
        CurrentViewModel = _studentsViewModel;
        await _studentsViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowEquipmentManagementAsync()
    {
        CurrentViewModel = _equipmentManagementViewModel;
        await _equipmentManagementViewModel.LoadAsync();
    }
}