using System.ComponentModel;
using PMB.Wpf.Client.ViewModel;

namespace PMB.Wpf.Client.View.BotSettings
{
    public partial class BotSettingsWindow
    {
        public BotSettingsWindow(BotSettingsViewModel context)
        {
            DataContext = context;
            InitializeComponent();
            Closing += BotSettings_Closing;
        }
        private void BotSettings_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
