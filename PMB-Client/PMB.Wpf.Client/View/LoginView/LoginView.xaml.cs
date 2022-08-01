using MahApps.Metro.Controls;
using PMB.Wpf.Client.ViewModel;

namespace PMB.Wpf.Client.View.LoginView
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : MetroWindow
    {
        public LoginView(LoginViewModel model)
        {
            DataContext = model;
            InitializeComponent();
        }
    }
}
