using csFloatTracker.Utils;

namespace csFloatTracker.ViewModel.InternalWindows;

public class BuyItemWindowVM : BindableBase
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

    public RelayCommand BuyCommand { get; }
    public event Action? OnWindowClosed;

    public BuyItemWindowVM()
    {
        BuyCommand = new RelayCommand(BuyCommandFnc, BuyCommandCE);
    }

    private bool BuyCommandCE(object? _) => !string.IsNullOrEmpty(Name) && Price != 0;
    private void BuyCommandFnc(object? _)
    {
        IsValid = true;
        OnWindowClosed?.Invoke();
    }
}
