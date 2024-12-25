using csFloatTracker.ViewModel.InternalWindows;
using System.Windows;

namespace csFloatTracker.UIControl.InternalWindows;

/// <summary>
/// Interaction logic for EditFloatItemWindow.xaml
/// </summary>
public partial class EditFloatItemWindow : Window
{
    private readonly EditFloatItemWindowVM? _vm;
    public EditFloatItemWindow()
    {
        InitializeComponent();

        if (DataContext is EditFloatItemWindowVM vm)
        {
            _vm = vm;
            _vm.OnWindowClosed += OnWindowClosed;
        }
    }
    private void OnWindowClosed() => Close();
}
