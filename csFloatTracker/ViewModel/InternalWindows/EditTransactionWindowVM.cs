using csFloatTracker.Model;
using csFloatTracker.Utils;

namespace csFloatTracker.ViewModel.InternalWindows;

public class EditTransactionWindowVM : BindableBase
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

    private DateTime _sellDate;
    public DateTime SellDate
    {
        get => _sellDate;
        set
        {
            _sellDate = value; OnPropertyChanged();
        }
    }

    private DateTime _buyDate;
    public DateTime BuyDate
    {
        get => _buyDate;
        set
        {
            _buyDate = value; OnPropertyChanged();
        }
    }

    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private float _float = 0;
    public float Float
    {
        get => _float;
        set
        {
            _float = value;
            OnPropertyChanged();
        }
    }

    private decimal _buyPrice = 0;
    public decimal BuyPrice
    {
        get => _buyPrice;
        set
        {
            _buyPrice = value;
            OnPropertyChanged();
        }
    }

    private decimal _sellPrice = 0;
    public decimal SellPrice
    {
        get => _sellPrice;
        set
        {
            _sellPrice = value;
            OnPropertyChanged();
        }
    }

    private decimal _tax = 0;
    public decimal Tax
    {
        get => _tax;
        set
        {
            _tax = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand EditCommand { get; }
    public event Action? OnWindowClosed;

    private TransactionItem? _transactionItem;

    public EditTransactionWindowVM()
    {
        EditCommand = new RelayCommand(EditCommandFnc, EditCommandCE);
    }

    public void InitializeTransaction(TransactionItem transactionItem)
    {
        _transactionItem = transactionItem;
        SellDate = _transactionItem.SoldDate;
        BuyDate = _transactionItem.CreatedDate;
        BuyPrice = _transactionItem.BuyPrice;
        SellPrice = _transactionItem.SoldPrice;
        Tax = _transactionItem.Tax;
        Float = _transactionItem.Float;
        Name = _transactionItem.Name ?? "";
    }

    private bool EditCommandCE(object? _) => IsDateValid(BuyDate) && IsDateValid(SellDate) && BuyPrice >= 0 && SellPrice >= 0 &&
        !string.IsNullOrEmpty(Name) && Tax >= 0 && Float >= 0 && Float <= 1;

    private void EditCommandFnc(object? _)
    {
        IsValid = true;
        HasChanges = SellDate != _transactionItem?.SoldDate ||
            BuyDate != _transactionItem?.CreatedDate || Name != _transactionItem.Name ||
            Float != _transactionItem.Float || Tax != _transactionItem.Tax ||
            BuyPrice != _transactionItem.BuyPrice || SellPrice != _transactionItem.SoldPrice;
        OnWindowClosed?.Invoke();
    }

    private static bool IsDateValid(DateTime date) => date != DateTime.MinValue && date != DateTime.MaxValue;
}
