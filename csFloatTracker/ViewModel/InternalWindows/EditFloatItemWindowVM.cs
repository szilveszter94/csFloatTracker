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

    private decimal _price = 0;
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
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
        BuyDate = _inventoryItem.Created;
        Price = _inventoryItem.Price;
        Float = _inventoryItem.Float;
        Name = _inventoryItem.Name ?? "";
    }

    private bool EditCommandCE(object? _) => !string.IsNullOrEmpty(Name) && Price >= 0 && 
        BuyDate != DateTime.MinValue && BuyDate != DateTime.MaxValue && Float >= 0 && Float <= 1;
    private void EditCommandFnc(object? _)
    {
        IsValid = true;
        HasChanges = BuyDate != _inventoryItem?.Created || 
            Price != _inventoryItem.Price || 
            Float != _inventoryItem.Float ||
            Name != _inventoryItem.Name;
        OnWindowClosed?.Invoke();
    }
}
