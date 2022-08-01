using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using NLog.Fluent;
using PMB.Domain.Logger;

namespace PMB.Wpf.Client.Infra
{
    public class PanelLogger : IPanelLogger
    {
        public ObservableCollection<LogItem> LogItems { get; set; } = new();
        
        public async Task AddInfoLog(string message)
        {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var logItem = new LogItem
                {
                    Info = message,
                    Time = DateTime.Now
                };
                LogItems.Insert(0,logItem);
                OnPropertyChanged(nameof(LogItems));
            });
        }

        public async Task AddForkInfo(string info, string team, string teams, string bookmakerName, string sportType)
        {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var logItem = new LogItem
                {
                    Time = DateTime.Now,
                    Info = info,
                    Team = team,
                    Teams = teams,
                    BookmakerName = bookmakerName,
                    SportType = sportType
                };
                LogItems.Insert(0,logItem);
                OnPropertyChanged(nameof(LogItems));
            });
        }

        public async Task Clear()
        {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                LogItems.Clear();
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}