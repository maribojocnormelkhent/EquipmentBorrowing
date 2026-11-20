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

public partial class EquipmentManagementViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;

    public ObservableCollection<Equipment> Items { get; } = new();

    [ObservableProperty] private string? searchText;
    [ObservableProperty] private bool availableOnly;
    [ObservableProperty] private Equipment? selectedItem;
    [ObservableProperty] private string? editIdentifier;
    [ObservableProperty] private string? editName;
    [ObservableProperty] private string? editDescription;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    public EquipmentManagementViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    partial void OnSelectedItemChanged(Equipment? value)
    {
        if (value is null)
            return;

        EditIdentifier = value.Identifier;
        EditName = value.Name;
        EditDescription = value.Description;
    }

    public async Task LoadAsync(int? reselectId = null)
    {
        IsBusy = true;
        try
        {
            var items = await _executor.ExecuteAsync<EquipmentManagementService, IReadOnlyList<Equipment>>(
                s => s.SearchAsync(SearchText, AvailableOnly));

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);

            if (reselectId.HasValue)
                SelectedItem = Items.FirstOrDefault(i => i.Id == reselectId.Value);

            if (items.Count == 0)
                StatusMessage = string.IsNullOrWhiteSpace(SearchText) && !AvailableOnly
                    ? "No equipment registered yet."
                    : "No equipment matches your search.";
        }
        catch (Exception)
        {
            StatusMessage = "Could not load equipment from the database. Check the connection and try again.";
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
        AvailableOnly = false;
        StatusMessage = null;
        await LoadAsync();
    }

    [RelayCommand]
    private void NewItem()
    {
        SelectedItem = null;
        EditIdentifier = null;
        EditName = null;
        EditDescription = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task RegisterAsync() =>
        RunAsync(s => s.RegisterAsync(new RegisterEquipmentRequest(EditIdentifier, EditName, EditDescription)));

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedItem is null)
        {
            StatusMessage = "Select an item from the list to edit first.";
            return;
        }

        var id = SelectedItem.Id;
        await RunAsync(s => s.UpdateAsync(
            new UpdateEquipmentRequest(id, EditIdentifier, EditName, EditDescription)));
    }

    private async Task RunAsync(Func<EquipmentManagementService, Task<OperationResult<Equipment>>> operation)
    {
        IsBusy = true;
        try
        {
            var result = await _executor.ExecuteAsync<EquipmentManagementService, OperationResult<Equipment>>(operation);
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