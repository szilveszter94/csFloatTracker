using csFloatTracker.ViewModel.InternalWindows;
using System.Windows;

namespace csFloatTracker.UIControl.InternalWindows;

/// <summary>
/// Interaction logic for EditAccountWindow.xaml
/// </summary>
public partial class EditAccountWindow : Window
{
    private readonly EditAccountWindowVM? _vm;
    public EditAccountWindow()
    {
        InitializeComponent();

        if (DataContext is EditAccountWindowVM vm)
        {
            _vm = vm;
            _vm.OnWindowClosed += OnWindowClosed;
        }
    }

    private void OnWindowClosed() => Close();
}
