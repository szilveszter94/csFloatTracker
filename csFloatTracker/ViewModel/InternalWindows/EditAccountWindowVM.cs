using csFloatTracker.Model;
using csFloatTracker.Utils;

namespace csFloatTracker.ViewModel.InternalWindows;

public class EditAccountWindowVM : BindableBase
{
    private bool _isValid = false;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            _isValid = value;
            OnPropertyChanged();
        }
    }

    private bool _hasChanges = false;
    public bool HasChanges
    {
        get => _hasChanges;
        set
        {
            _hasChanges = value;
            OnPropertyChanged();
        }
    }

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

    public RelayCommand EditCommand { get; }
    public event Action? OnWindowClosed;

    private CsAccount? _account;

    public EditAccountWindowVM()
    {
        EditCommand = new RelayCommand(EditCommandFnc, EditCommandCE);
    }

    public void InitializeAccount(CsAccount account)
    {
        _account = account;
        SoldCount = _account.SoldCount;
        Balance = _account.Balance;
        Profit = _account.Profit;
        PurchasedCount = _account.PurchasedCount;
    }

    private bool EditCommandCE(object? _) => true;
    private void EditCommandFnc(object? _)
    {
        IsValid = true;
        HasChanges = SoldCount != _account?.SoldCount ||
            Profit != _account?.Profit ||
            Balance != _account?.Balance ||
            PurchasedCount != _account?.PurchasedCount;
        OnWindowClosed?.Invoke();
    }
}
