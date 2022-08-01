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

namespace PMB.Wpf.Client.View.Templates
{
    /// <summary>
    /// Логика взаимодействия для StatisticItem.xaml
    /// </summary>
    public partial class StatisticItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(StatisticItem));
        public string Title
        {
            get
            {
                return GetValue(TitleProperty) as string;
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        
        public static readonly DependencyProperty CounterProperty = DependencyProperty.Register("Counter", typeof(string), typeof(StatisticItem));
        public string Counter
        {
            get
            {
                return GetValue(CounterProperty) as string;
            }
            set
            {
                SetValue(CounterProperty, value);
            }
        }
        public StatisticItem()
        {
            InitializeComponent();
        }
    }
}
