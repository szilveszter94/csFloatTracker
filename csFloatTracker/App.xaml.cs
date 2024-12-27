using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Windows;

namespace csFloatTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ILogger Logger { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddNLog();
            });

            Logger = loggerFactory.CreateLogger("WPFApp");
            Logger.LogInformation("The application is started.");
        }
    }
}
