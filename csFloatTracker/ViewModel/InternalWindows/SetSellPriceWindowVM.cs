using csFloatTracker.Utils;

namespace csFloatTracker.ViewModel.InternalWindows;

public class SetSellPriceWindowVM : BindableBase
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

    private decimal _boughtPrice = 0;
    public decimal BoughtPrice
    {
        get => _boughtPrice;
        set
        {
            _boughtPrice = value;
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

    public RelayCommand SellCommand { get; }
    public event Action? OnWindowClosed;

    public SetSellPriceWindowVM()
    {
        SellCommand = new RelayCommand(SellCommandFnc, SellCommandCE);
    }

    private bool SellCommandCE(object? _) => SellPrice > 0;
    private void SellCommandFnc(object? _)
    {
        IsValid = true;
        OnWindowClosed?.Invoke();
    }
}
