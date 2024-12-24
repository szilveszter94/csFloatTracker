using csFloatTracker.ViewModel.InternalWindows;
using System.Windows;

namespace csFloatTracker.UIControl;

/// <summary>
/// Interaction logic for SetSellPriceWindow.xaml
/// </summary>
public partial class SetSellPriceWindow : Window
{
    private readonly SetSellPriceWindowVM? _vm;

    public SetSellPriceWindow()
    {
        InitializeComponent();

        if (DataContext is SetSellPriceWindowVM vm)
        {
            _vm = vm;
            _vm.OnWindowClosed += OnWindowClosed;
        }
    }

    private void OnWindowClosed() => Close();
}
