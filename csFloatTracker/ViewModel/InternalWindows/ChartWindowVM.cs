using csFloatTracker.Model;
using csFloatTracker.Utils;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Globalization;

namespace csFloatTracker.ViewModel.InternalWindows;

public class ChartWindowVM : BindableBase
{
    private ObservableCollection<TransactionItem> _transactions = [];
    public ObservableCollection<TransactionItem> Transactions
    {
        get => _transactions;
        set
        {
            _transactions = value;
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

    public ChartWindowVM()
    {
    }

    public void InitializeTransactions(ObservableCollection<TransactionItem> transactionItems)
    {
        Transactions.Clear();
        foreach (var transactionItem in transactionItems)
        {
            Transactions.Add(transactionItem);
        }
        UpdateChart();
    }

    private void UpdateChart()
    {
        var profits = Transactions.Select(t => t.Profit).ToList();
        Dates = new ObservableCollection<string>(Transactions.Select(t => t.CreatedDate.ToString("MM/dd/yyyy")));

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
