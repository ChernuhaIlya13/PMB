using System.Threading;
using PMB.Wpf.Client.ViewModel;

namespace PMB.Wpf.Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(MainWindowViewModel context)
        {
            DataContext = context;
            InitializeComponent();
            Closing += (sender, args) =>
            {
                System.Windows.Application.Current.Shutdown();
            };
            Loaded += (sender, args) =>
            {
                context.BrowserOperations.CreateAllBrowsersWithoutLogin(CancellationToken.None);
            };
        }
    }
}
