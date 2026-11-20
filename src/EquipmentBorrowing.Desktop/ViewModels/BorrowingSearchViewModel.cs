using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingSearchViewModel : ObservableObject
{
    private readonly IUseCaseExecutor _executor;
    private int _requestId;

    public IReadOnlyList<BorrowingStatusFilter> StatusOptions { get; } = Enum.GetValues<BorrowingStatusFilter>();
    public IReadOnlyList<BorrowingSortField> SortFieldOptions { get; } = Enum.GetValues<BorrowingSortField>();
    public IReadOnlyList<SortDirection> DirectionOptions { get; } = Enum.GetValues<SortDirection>();

    public ObservableCollection<BorrowingDetail> Records { get; } = new();

    [ObservableProperty] private string? searchText;
    [ObservableProperty] private BorrowingStatusFilter selectedStatus = BorrowingStatusFilter.All;
    [ObservableProperty] private BorrowingSortField selectedSortField = BorrowingSortField.CheckoutDate;
    [ObservableProperty] private SortDirection selectedDirection = SortDirection.Descending;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? message;
    [ObservableProperty] private bool messageIsError;

    public BorrowingSearchViewModel(IUseCaseExecutor executor)
    {
        _executor = executor;
    }

    public async Task LoadAsync()
    {
        var request = ++_requestId;
        var criteria = new BorrowingSearchCriteria(SearchText, SelectedStatus, SelectedSortField, SelectedDirection);

        IsLoading = true;
        Message = null;
        MessageIsError = false;

        try
        {
            var items = await _executor.ExecuteAsync<BorrowingQueryService, IReadOnlyList<BorrowingDetail>>(
                s => s.SearchAsync(criteria));

            if (request != _requestId)
                return;

            Records.Clear();
            foreach (var item in items)
                Records.Add(item);

            Message = items.Count == 0
                ? "No borrowing records match your criteria."
                : $"{items.Count} record(s) found.";
        }
        catch (Exception)
        {
            if (request != _requestId)
                return;

            Records.Clear();
            Message = "Could not load borrowing records. Check the database connection and try again.";
            MessageIsError = true;
        }
        finally
        {
            if (request == _requestId)
                IsLoading = false;
        }
    }

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    [RelayCommand]
    private Task ResetAsync()
    {
        SearchText = null;
        SelectedStatus = BorrowingStatusFilter.All;
        SelectedSortField = BorrowingSortField.CheckoutDate;
        SelectedDirection = SortDirection.Descending;
        return LoadAsync();
    }
}