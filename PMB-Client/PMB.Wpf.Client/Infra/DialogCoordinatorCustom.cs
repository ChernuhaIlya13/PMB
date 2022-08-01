using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace PMB.Wpf.Client.Infra
{
    public class DialogCoordinatorCustom
    {
        public IDialogCoordinator DialogCoordinator;
        
        public DialogCoordinatorCustom(IDialogCoordinator coordinator)
        {
            DialogCoordinator = coordinator;
        }
        
        public async Task ShowMessageAsyncDefault(object context, string title, string message)
        {
            await DialogCoordinator.ShowMessageAsync(context, title, message, MessageDialogStyle.Affirmative, new MetroDialogSettings() { AnimateHide = true, AnimateShow = true, ColorScheme = MetroDialogColorScheme.Accented, DialogTitleFontSize = 30.0 });
        } 
    }
}
