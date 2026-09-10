using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>
/// Handles navigation between the Equipment and Active Borrowings sections.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    [ObservableProperty]
    private object? currentViewModel;

    public MainWindowViewModel(EquipmentViewModel equipmentViewModel, BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private async Task ShowEquipmentAsync()
    {
        await _equipmentViewModel.LoadAsync();
        CurrentViewModel = _equipmentViewModel;
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        await _borrowingsViewModel.LoadAsync();
        CurrentViewModel = _borrowingsViewModel;
    }
}