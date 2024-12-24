using System.Windows.Input;

namespace csFloatTracker.Utils;

public class RelayCommand : ICommand
{
    private Action<object?> _command;
    private Predicate<object?> _canExecute;
    private readonly object? _storedParameter;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<object?> command, Predicate<object?> canExecute, bool subscribe = true, object? parameter = null)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
        _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        _storedParameter = parameter;

        if (subscribe)
        {
            SubscribeEvent();
        }
    }

    public RelayCommand(Action<object?> command)
        : this(command, DefaultCanExecute)
    {
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (_command == null)
        {
            throw new InvalidOperationException("Command is not initialized.");
        }

        _command(_storedParameter ?? parameter);
    }

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SubscribeEvent()
    {
        CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
        CommandManager.RequerySuggested += CommandManager_RequerySuggested;
    }

    public void ReleaseEvent()
    {
        CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
    }

    public void Destroy()
    {
        ReleaseEvent();
        _canExecute = _ => false;
        _command = _ => { };
    }

    private void CommandManager_RequerySuggested(object? sender, EventArgs e)
    {
        OnCanExecuteChanged();
    }

    private static bool DefaultCanExecute(object? parameter)
    {
        return true;
    }
}

