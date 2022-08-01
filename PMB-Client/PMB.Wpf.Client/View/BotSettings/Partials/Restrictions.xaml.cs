using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMB.Wpf.Client.View.BotSettings.Partials
{
    /// <summary>
    /// Логика взаимодействия для Restrictions.xaml
    /// </summary>
    public partial class Restrictions : UserControl
    {
        public Restrictions()
        {
            InitializeComponent();
            FirstItem.MouseMove += FirstItem_MouseMove;
            SecondItem.MouseMove += SecondItem_MouseMove;
            ThirdItem.MouseMove += ThirdItem_MouseMove;
            FourthItem.MouseMove += FourthItem_MouseMove;
        }

        private void FirstItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(FirstItem, new DataObject("Марафон"), DragDropEffects.Copy);
            }
        }
        private void SecondItem_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(SecondItem, new DataObject("Лига ставок"), DragDropEffects.Copy);
            }
        }
        private void ThirdItem_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(ThirdItem, new DataObject("Pinnacle"), DragDropEffects.Copy);
            }
        }
        private void FourthItem_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(FourthItem, new DataObject("bet365"), DragDropEffects.Copy);
            }
        }
        
        private void HandleDrops(object sender, DragEventArgs e)
        {
            IDataObject data = e.Data as IDataObject;
            var d = (string)data.GetData(DataFormats.UnicodeText);
            var destination = sender as TextBlock;
            destination.Text = d;
            destination.FontSize = 16;
            destination.VerticalAlignment = VerticalAlignment.Center;
            destination.Padding = new Thickness(0, 35, 0, 0);
        }

        private void FirstTeamRectangle_Drop_1(object sender, DragEventArgs e)
        {
            HandleDrops(sender, e);
        }

        private void SecondTeamRectangle_Drop_1(object sender, DragEventArgs e)
        {
            HandleDrops(sender, e);
        }

        private void ListBookmakers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = (sender as ListBox).SelectedItems as List;

        }
    }
}
