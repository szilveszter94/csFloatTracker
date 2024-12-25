using csFloatTracker.ViewModel.CenterPanel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace csFloatTracker.UIControl.CenterPanel;

/// <summary>
/// Interaction logic for CsFloatTracker.xaml
/// </summary>
public partial class CsFloatTracker : UserControl
{
    private readonly CsFloatTrackerVM? _vm;

    public CsFloatTracker()
    {
        InitializeComponent();
        Loaded += (s, e) => Focus();

        if (DataContext is CsFloatTrackerVM vm)
        {
            _vm = vm;
        }
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            _vm?.HandleShortcut(ShortcutType.CtrlB);
            return;
        }
        if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            _vm?.HandleShortcut(ShortcutType.CtrlS);
            return;
        }
    }
}

public enum ShortcutType
{
    CtrlB,
    CtrlS
}
