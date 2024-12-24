using csFloatTracker.ViewModel.InternalWindows;
using System.Windows;

namespace csFloatTracker.UIControl.InternalWindows;

/// <summary>
/// Interaction logic for BuyItemWindow.xaml
/// </summary>
public partial class BuyItemWindow : Window
{
    private readonly BuyItemWindowVM? _vm;

    public BuyItemWindow()
    {
        InitializeComponent();

        if (DataContext is BuyItemWindowVM vm)
        {
            _vm = vm;
            _vm.OnWindowClosed += OnWindowClosed;
        }
    }

    private void OnWindowClosed() => Close();
}
