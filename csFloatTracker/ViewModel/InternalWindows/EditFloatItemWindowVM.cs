using csFloatTracker.Model;
using csFloatTracker.Utils;

namespace csFloatTracker.ViewModel.InternalWindows;

public class EditFloatItemWindowVM : BindableBase
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

    private InventoryItem? _inventoryItem;

    public EditFloatItemWindowVM()
    {
        EditCommand = new RelayCommand(EditCommandFnc, EditCommandCE);
    }

    public void InitializeInventoryItem(InventoryItem inventoryItem)
    {
        _inventoryItem = inventoryItem;
        BuyDate = inventoryItem.Created;
    }

    private bool EditCommandCE(object? _) => true;
    private void EditCommandFnc(object? _)
    {
        IsValid = true;
        HasChanges = BuyDate != _inventoryItem?.Created;
        OnWindowClosed?.Invoke();
    }
}
