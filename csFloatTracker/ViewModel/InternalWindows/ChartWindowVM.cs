using csFloatTracker.Model;
using csFloatTracker.Utils;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Globalization;

namespace csFloatTracker.ViewModel.InternalWindows;

public class ChartWindowVM : BindableBase
{
    private ObservableCollection<TransactionItem> _filteredTransactions = [];
    public ObservableCollection<TransactionItem> FilteredTransactions
    {
        get => _filteredTransactions;
        set
        {
            _filteredTransactions = value;
            OnPropertyChanged();
        }
    }

    private SeriesCollection _profitSeries = [];
    public SeriesCollection ProfitSeries
    {
        get => _profitSeries;
        set
        {
            _profitSeries = value;
            OnPropertyChanged(nameof(ProfitSeries));
        }
    }

    private ObservableCollection<string> _dates = [];
    public ObservableCollection<string> Dates
    {
        get => _dates;
        set
        {
            _dates = value;
            OnPropertyChanged(nameof(Dates));
        }
    }private Func<double, string>? _yFormatter;
    public Func<double, string>? YFormatter
    {
        get => _yFormatter;
        set
        {
            _yFormatter = value;
            OnPropertyChanged(nameof(YFormatter));
        }
    }

    public RelayCommand SessionChangeCommand { get; }
    private ChartSession _selectedSession = ChartSession.AllTime;
    private ObservableCollection<TransactionItem> _transactionList = [];

    public ChartWindowVM()
    {
        SessionChangeCommand = new RelayCommand(SessionChangeCommandFnc, SessionChangeCommandCE);
    }

    private bool SessionChangeCommandCE(object? _) => true;
    private void SessionChangeCommandFnc(object? param)
    {
        if (param is ChartSession session)
        {
            if (session == _selectedSession) return;
            _selectedSession = session;
            FilterTransactionList(session);
        }
    }

    private void FilterTransactionList(ChartSession session)
    {
        DateTime now = DateTime.Now;
        DateTime startDate = session switch
        {
            ChartSession.Daily => now.Date,
            ChartSession.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            ChartSession.Monthly => new DateTime(now.Year, now.Month, 1),
            ChartSession.Yearly => new DateTime(now.Year, 1, 1),
            ChartSession.AllTime => DateTime.MinValue,
            _ => DateTime.MinValue
        };

        var filteredTransactions = _transactionList.Where(t => t.SoldDate >= startDate);

        FilteredTransactions.Clear();
        foreach (var transactionItem in filteredTransactions)
        {
            FilteredTransactions.Add(transactionItem);
        }

        UpdateChart();
    }

    public void InitializeTransactions(ObservableCollection<TransactionItem> transactionItems)
    {
        _transactionList = transactionItems;
        FilteredTransactions.Clear();
        foreach (var transactionItem in _transactionList)
        {
            FilteredTransactions.Add(transactionItem);
        }
        UpdateChart();
    }

    private void UpdateChart()
    {
        var profits = FilteredTransactions.Select(t => t.Profit).ToList();
        Dates = new ObservableCollection<string>(FilteredTransactions.Select(t => t.SoldDate.ToString("MM/dd/yyyy")));

        ProfitSeries = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Profit",
                Values = new ChartValues<decimal>(profits)
            }
        };

        YFormatter = value => value.ToString("C", CultureInfo.GetCultureInfo("en-US"));
    }
}
public enum ChartSession
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
    AllTime
}
