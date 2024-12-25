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
        SellDate = transactionItem.SoldDate;
        BuyDate = transactionItem.CreatedDate;
    }

    private bool EditCommandCE(object? _) => true;
    private void EditCommandFnc(object? _)
    {
        IsValid = true;
        HasChanges = SellDate != _transactionItem?.SoldDate ||
            BuyDate != _transactionItem?.CreatedDate;
        OnWindowClosed?.Invoke();
    }
}
