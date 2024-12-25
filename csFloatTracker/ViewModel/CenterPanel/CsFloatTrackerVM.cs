using csFloatTracker.Context;
using csFloatTracker.Model;
using csFloatTracker.Repository;
using csFloatTracker.UIControl;
using csFloatTracker.UIControl.InternalWindows;
using csFloatTracker.Utils;
using csFloatTracker.ViewModel.InternalWindows;
using System.Collections.ObjectModel;
using System.Windows;

namespace csFloatTracker.ViewModel.CenterPanel;

public class CsFloatTrackerVM : BindableBase
{
    private int _soldCount;
    public int SoldCount
    {
        get => _soldCount;
        set
        {
            _soldCount = value; OnPropertyChanged();
        }
    }

    private int _purchasedCount;
    public int PurchasedCount
    {
        get => _purchasedCount;
        set
        {
            _purchasedCount = value; OnPropertyChanged();
        }
    }

    private decimal _balance;
    public decimal Balance
    {
        get => _balance;
        set
        {
            _balance = value; OnPropertyChanged();
        }
    }

    private decimal _profit;
    public decimal Profit
    {
        get => _profit;
        set
        {
            _profit = value; OnPropertyChanged();
        }
    }

    private decimal _tax;
    public decimal Tax
    {
        get => _tax;
        set
        {
            _tax = value; OnPropertyChanged();
        }
    }

    private InventoryItem? _selectedInventoryItem;
    public InventoryItem? SelectedInventoryItem
    {
        get => _selectedInventoryItem;
        set
        {
            _selectedInventoryItem = value;

            if (SelectedTransactionItem != null)
            {
                SelectedTransactionItem = null;
            }
            OnPropertyChanged();
        }
    }

    private TransactionItem? _selectedTransactionItem;
    public TransactionItem? SelectedTransactionItem
    {
        get => _selectedTransactionItem;
        set
        {
            _selectedTransactionItem = value;

            if (SelectedInventoryItem != null)
            {
                SelectedInventoryItem = null;
            }
            OnPropertyChanged();
        }
    }

    private ObservableCollection<InventoryItem> _inventory = [];
    public ObservableCollection<InventoryItem> Inventory
    {
        get => _inventory;
        set { _inventory = value; OnPropertyChanged(); }
    }

    private ObservableCollection<TransactionItem> _transactionHistroy = [];
    public ObservableCollection<TransactionItem> TransactionHistroy
    {
        get => _transactionHistroy;
        set { _transactionHistroy = value; OnPropertyChanged(); }
    }

    public RelayCommand BuyCommand { get; }
    public RelayCommand SellCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    private readonly FloatTrackerContext _context;
    private readonly FloatTrackerRepository _repository;
    private CsAccount? _account;

    public CsFloatTrackerVM()
    {
        _context = new FloatTrackerContext();
        _repository = new FloatTrackerRepository(_context);

        BuyCommand = new RelayCommand(BuyCommandFnc, BuyCommandCE);
        SellCommand = new RelayCommand(SellCommandFnc, SellCommandCE);
        EditCommand = new RelayCommand(EditCommandFnc, EditCommandCE);
        DeleteCommand = new RelayCommand(DeleteCommandFnc, DeleteCommandCE);
        RefreshAsync();
    }

    private bool BuyCommandCE(object? _) => true;
    private void BuyCommandFnc(object? _)
    {
        var buyWindow = new BuyItemWindow();
        buyWindow.ShowDialog();
        OnBuyWindowClosed(buyWindow);
    }

    private async void OnBuyWindowClosed(BuyItemWindow window)
    {
        if (window.DataContext is BuyItemWindowVM vm && vm.IsValid)
        {
            var floatItem = new InventoryItem() { Name = vm.Name, Price = vm.Price, Float = vm.Float, CsAccountId = _account?.Id ?? 1 };
            await _repository.BuyFloatAsync(floatItem);
            RefreshAsync();
        }
    }

    private bool SellCommandCE(object? _) => SelectedInventoryItem != null;
    private void SellCommandFnc(object? _)
    {
        if (SelectedInventoryItem != null)
        {
            var setSellPriceWindow = new SetSellPriceWindow();
            if (setSellPriceWindow.DataContext is SetSellPriceWindowVM vm)
            {
                vm.BoughtPrice = SelectedInventoryItem.Price;
                vm.Tax = _account == null ? 0 : _account.Tax;
            }
            setSellPriceWindow.ShowDialog();
            OnSellWindowClosed(setSellPriceWindow);
        }
    }

    private async void OnSellWindowClosed(SetSellPriceWindow window)
    {
        if (SelectedInventoryItem != null && window.DataContext is SetSellPriceWindowVM vm && vm.IsValid)
        {
            await _repository.SellFloatAsync(SelectedInventoryItem, vm);
            RefreshAsync();
        }
    }

    private bool EditCommandCE(object? _) => _account != null;
    private void EditCommandFnc(object? _)
    {
        if (_account == null)
        {
            return;
        }

        var editAccountWindow = new EditAccountWindow();
        if (editAccountWindow.DataContext is EditAccountWindowVM vm)
        {
            vm.InitializeAccount(_account);
        }
        editAccountWindow.ShowDialog();
        OnEditWindowClosed(editAccountWindow);
    }

    private async void OnEditWindowClosed(EditAccountWindow window)
    {
        if (_account != null && window.DataContext is EditAccountWindowVM vm &&
            vm.HasChanges && vm.IsValid)
        {
            await _repository.UpdateAccountAsync(_account, vm);
            RefreshAsync();
        }
    }

    private bool DeleteCommandCE(object? _) => SelectedInventoryItem != null || SelectedTransactionItem != null;
    private async void DeleteCommandFnc(object? _)
    {
        if (SelectedInventoryItem != null)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the inventory item?",
                                          "Delete inventory item",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _repository.DeleteInventoryItem(SelectedInventoryItem);
            }
            RefreshAsync();
        }
        else if (SelectedTransactionItem != null)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the transaction item?",
                                          "Delete transaction",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _repository.DeleteTransactionItem(SelectedTransactionItem);
            }
            RefreshAsync();
        }
    }

    private async void RefreshAsync()
    {
        _account = await _repository.GetAccountAsync();
        Profit = _account.Profit;
        Balance = _account.Balance;
        SoldCount = _account.SoldCount;
        PurchasedCount = _account.PurchasedCount;
        Tax = _account.Tax;

        Inventory.Clear();
        foreach (var item in _account.Inventory)
        {
            Inventory.Add(item);
        }

        TransactionHistroy.Clear();
        foreach (var item in _account.TransactionHistory)
        {
            TransactionHistroy.Add(item);
        }
    }
}
