using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Browsers.Common;
using PMB.Cef.Core.FakeConfig;
using PMB.Domain.BrowserModels;
using PMB.Utils;
using PMB.Wpf.Client.Annotations;
using PMB.Wpf.Client.Commands;
using PMB.Wpf.Client.Operations;

namespace PMB.Wpf.Client.ViewModel
{
    public class BotSettingsViewModel: INotifyPropertyChanged
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IOptions<SettingsOptions> _options;
        private BrowserOperations _browserOperations;

        public BotSettingsViewModel(ISettingsProvider provider,IOptions<SettingsOptions> options,BrowserOperations browserOperations)
        {
            _options = options;
            _settingsProvider = provider;
            Settings = _settingsProvider.GetSettings();
            SaveSettings = new RelayCommand<object>(SaveSettingsCommand);
            AddItemToList = new RelayCommand<object>(AddItemToListCommand);
            RemoveItemFromList = new RelayCommand<object>(RemoveItemFromListCommand);
            ChangeActiveBookmaker = new RelayCommand<string>(ChangeActiveBookmakerCommand);
            GenerateNewFakeProfile = new RelayCommand<object>(async (o) =>
            {
                await GenerateNewFakeProfileCommand(o);
            });
            DisableEnableBookmaker = new RelayCommand<string>(DisableEnableBookmakerCommand);
            _browserOperations = browserOperations;
            RemoveDataOfBrowser = new RelayCommand<object>(RemoveDataOfBrowserCommand);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ForkSettings Settings { get; set; }
        
        public Bookmaker CurrentActiveBookmaker { get; set; }
        public ICommand ChangeActiveBookmaker { get; set; }

        public ICommand SaveSettings { get; set; }

        public ICommand AddItemToList { get; set; }

        public ICommand RemoveItemFromList { get; set; }

        public ICommand GenerateNewFakeProfile { get; set; }
        
        public ICommand DisableEnableBookmaker { get; set; }
        
        public ICommand RemoveDataOfBrowser { get; set; }

        public string FirstSelectedTeam { get; set; } = "+";

        public string SecondSelectedTeam { get; set; } = "+";

        public ForkSettings.SequenceBookmakers SelectedTeam { get; set; }

        private void RemoveDataOfBrowserCommand(object o)
        {
            Process.Start(System.Windows.Application.ResourceAssembly.Location);
            System.Windows.Application.Current.Shutdown();
            if (CurrentActiveBookmaker != null)
            {
                var cachePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\CefSharp\\Cache\\{CurrentActiveBookmaker.BookmakerName}";
                try
                {
                    if (Directory.Exists(cachePath))
                    {
                        Directory.Delete(cachePath,recursive:true);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        private async Task GenerateNewFakeProfileCommand(object o)
        {
            if(CurrentActiveBookmaker != null)
            {
                var newFakeProfile = await FakeProfileFactory.Generate(CurrentActiveBookmaker);
                newFakeProfile.DisableAnonym = CurrentActiveBookmaker.BrowserOptions.Anonym.DisableAnonym;
                CurrentActiveBookmaker.BrowserOptions.Anonym = newFakeProfile;
                OnPropertyChanged(nameof(CurrentActiveBookmaker));
            }
        }

        private void RemoveItemFromListCommand(object o)
        {
            if(SelectedTeam != null)
            {
                Settings.SequenceRulesBookmakers.Remove(SelectedTeam);
            }
        }

        private void AddItemToListCommand(object o)
        {
            if(FirstSelectedTeam  != null && SecondSelectedTeam != null)
            {
                var sequence = new ForkSettings.SequenceBookmakers()
                {
                    Id = Settings.SequenceRulesBookmakers.Count,
                    First = FirstSelectedTeam,
                    Second = SecondSelectedTeam
                };
                Settings.SequenceRulesBookmakers.Add(sequence);
            }
        }
        
        public void ChangeActiveBookmakerCommand(string bookmaker)
        {
            CurrentActiveBookmaker = HandleBookmaker(_options.Value.Bookmakers.First(b => b.Bookmaker == bookmaker).Bookmaker);
        }

        private void SaveSettingsCommand(object o)
        {
            _settingsProvider.Save(Settings);
        }
        
        private Bookmaker HandleBookmaker(string bookmakerName)
        {
            var b = new Bookmaker
            {
                BookmakerName = bookmakerName
            };
            
            if (Settings.Bookmakers?.Any() == false)
            {
                Settings.Bookmakers = new List<Bookmaker> { b };
                return b;
            }
            return Settings?
                .Bookmakers?
                .FirstOrDefault(book => book.BookmakerName == bookmakerName);
        }

        public async void DisableEnableBookmakerCommand(string bookmakerName)
        {
            var bookmaker = Settings.Bookmakers.FirstOrDefault(b => b.BookmakerName == bookmakerName);
            if (!bookmaker.IsActive)
            {
                ViewModelUpdater.BrowserRemove.OnNext(bookmaker.BookmakerName);
            }
            else
            {
                await _browserOperations.CreateBookmaker(bookmakerName);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}