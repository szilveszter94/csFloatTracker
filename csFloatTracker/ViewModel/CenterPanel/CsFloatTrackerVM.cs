using csFloatTracker.Context;
using csFloatTracker.Model;
using csFloatTracker.Repository;
using csFloatTracker.UIControl;
using csFloatTracker.UIControl.CenterPanel;
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

            if (_suppressUpdates)
            {
                OnPropertyChanged();
                _suppressUpdates = false;
                return;
            }

            _selectedInventoryItem = value;

            if (SelectedTransactionItem != null)
            {
                _suppressUpdates = true;
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

            if (_suppressUpdates)
            {
                OnPropertyChanged();
                _suppressUpdates = false;
                return;
            }

            if (SelectedInventoryItem != null)
            {
                _suppressUpdates = true;
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
    public ObservableCollection<TransactionItem> TransactionHistory
    {
        get => _transactionHistroy;
        set { _transactionHistroy = value; OnPropertyChanged(); }
    }

    private ChartWindowVM? _chartVM;
    public ChartWindowVM? ChartVM
    {
        get => _chartVM;
        set { _chartVM = value; OnPropertyChanged(); }
    }

    public RelayCommand BuyCommand { get; }
    public RelayCommand SellCommand { get; }
    public RelayCommand EditAccountCommand { get; }
    public RelayCommand EditFloatCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ShowChartCommand { get; }

    private readonly FloatTrackerContext _context;
    private readonly FloatTrackerRepository _repository;
    private CsAccount? _account;
    private bool _suppressUpdates = false;

    public CsFloatTrackerVM()
    {
        _context = new FloatTrackerContext();
        _repository = new FloatTrackerRepository(_context);

        BuyCommand = new RelayCommand(BuyCommandFnc, BuyCommandCE);
        SellCommand = new RelayCommand(SellCommandFnc, SellCommandCE);
        EditAccountCommand = new RelayCommand(EditAccountCommandFnc, EditAccountCommandCE);
        EditFloatCommand = new RelayCommand(EditFloatCommandFnc, EditFloatCommandCE);
        DeleteCommand = new RelayCommand(DeleteCommandFnc, DeleteCommandCE);
        ShowChartCommand = new RelayCommand(ShowChartCommandFnc, ShowChartCommandCE);
        RefreshAsync();
    }

    public void HandleShortcut(ShortcutType shortcutType)
    {
        if (shortcutType == ShortcutType.CtrlB && BuyCommandCE(new object()))
        {
            BuyCommandFnc(new object());
            return;
        }
        if (shortcutType == ShortcutType.CtrlS && SellCommandCE(new object()))
        {
            SellCommandFnc(new object());
            return;
        }
    }

    private bool BuyCommandCE(object? _) => true;
    private void BuyCommandFnc(object? _)
    {
        var buyWindow = new BuyItemWindow
        {
            Owner = Application.Current.MainWindow
        };
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
            var setSellPriceWindow = new SetSellPriceWindow
            {
                Owner = Application.Current.MainWindow
            };
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

    private bool EditAccountCommandCE(object? _) => _account != null;
    private void EditAccountCommandFnc(object? _)
    {
        if (_account == null)
        {
            return;
        }

        var editAccountWindow = new EditAccountWindow
        {
            Owner = Application.Current.MainWindow
        };
        if (editAccountWindow.DataContext is EditAccountWindowVM vm)
        {
            vm.InitializeAccount(_account);
        }
        editAccountWindow.ShowDialog();
        OnEditAccountWindowClosed(editAccountWindow);
    }

    private async void OnEditAccountWindowClosed(EditAccountWindow window)
    {
        if (_account != null && window.DataContext is EditAccountWindowVM vm &&
            vm.HasChanges && vm.IsValid)
        {
            await _repository.UpdateAccountAsync(_account, vm);
            RefreshAsync();
        }
    }

    private bool EditFloatCommandCE(object? _) => SelectedInventoryItem != null || SelectedTransactionItem != null;
    private void EditFloatCommandFnc(object? _)
    {
        if (SelectedTransactionItem != null)
        {
            var editTransactionWindow = new EditTransactionWindow
            {
                Owner = Application.Current.MainWindow
            };
            if (editTransactionWindow.DataContext is EditTransactionWindowVM vm)
            {
                vm.InitializeTransaction(SelectedTransactionItem);
            }
            editTransactionWindow.ShowDialog();
            OnEditTransactionWindowClosed(editTransactionWindow);
            return;
        }

        if (SelectedInventoryItem != null)
        {
            var editFloatItemWindow = new EditFloatItemWindow
            {
                Owner = Application.Current.MainWindow
            };
            if (editFloatItemWindow.DataContext is EditFloatItemWindowVM vm)
            {
                vm.InitializeInventoryItem(SelectedInventoryItem);
            }
            editFloatItemWindow.ShowDialog();
            OnEditFloatWindowClosed(editFloatItemWindow);
            return;
        }
    }

    private async void OnEditFloatWindowClosed(EditFloatItemWindow window)
    {
        if (SelectedInventoryItem != null && window.DataContext is EditFloatItemWindowVM vm &&
            vm.HasChanges && vm.IsValid)
        {
            await _repository.UpdateInventoryItemAsync(SelectedInventoryItem, vm);
            RefreshAsync();
        }
    }

    private async void OnEditTransactionWindowClosed(EditTransactionWindow window)
    {
        if (SelectedTransactionItem != null && window.DataContext is EditTransactionWindowVM vm &&
            vm.HasChanges && vm.IsValid)
        {
            await _repository.UpdateTransactionAsync(SelectedTransactionItem, vm);
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

    private bool ShowChartCommandCE(object? _) => TransactionHistory.Any();
    private void ShowChartCommandFnc(object? _)
    {
        if (!TransactionHistory.Any())
        {
            return;
        }

        var chartWindow = new ChartWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (chartWindow.DataContext is ChartWindowVM vm)
        {
            vm.InitializeTransactions(TransactionHistory);
        }

        chartWindow.ShowDialog();
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

        TransactionHistory.Clear();
        foreach (var item in _account.TransactionHistory)
        {
            TransactionHistory.Add(item);
        }
    }
}
