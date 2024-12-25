using csFloatTracker.ViewModel.InternalWindows;
using System.Windows;

namespace csFloatTracker.UIControl.InternalWindows;

/// <summary>
/// Interaction logic for EditTransactionWindow.xaml
/// </summary>
public partial class EditTransactionWindow : Window
{
    private readonly EditTransactionWindowVM? _vm;

    public EditTransactionWindow()
    {
        InitializeComponent();

        if (DataContext is EditTransactionWindowVM vm)
        {
            _vm = vm;
            _vm.OnWindowClosed += OnWindowClosed;
        }
    }

    private void OnWindowClosed() => Close();
}
