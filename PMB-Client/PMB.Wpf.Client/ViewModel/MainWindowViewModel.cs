using System;
using PMB.Wpf.Client.Utilities;
using System.Windows.Input;
using PMB.Wpf.Client.View.BotSettings;
using PositiveFork = PMB.Models.PositiveModels.Fork;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using PMB.Wpf.Client.Annotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using PMB.Abb.Client.Providers;
using PMB.Application;
using PMB.Application.Commands;
using PMB.Application.Commands.Handlers;
using PMB.Application.Interfaces;
using PMB.Application.Queries;
using PMB.Application.Queries.Handlers;
using PMB.Domain;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;
using PMB.Models.PositiveModels;
using PMB.Utils;
using PMB.Wpf.Client.Commands;
using PMB.Wpf.Client.Operations;
using PMB.Wpf.Client.Services;

#pragma warning disable CS4014

namespace PMB.Wpf.Client.ViewModel
{
    
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IMediator _mediator;
        private ISettingsProvider _settingsProvider;
        private readonly ForksServiceProvider _forksProvider;
        private readonly IAbbForksProvider _abbForksProvider;
        public readonly BrowserOperations BrowserOperations;
        private readonly ForkBetOperations _forkBetOperations;
        private readonly int _thousand = 1000;
        private CancellationTokenSource _token = new();
        
        private static readonly PMB.Utilities.AsyncLock Lock = new();
        private static readonly object SyncLock = new();
        private bool _botProcessForkNow;
        private bool _botLaunching;
        private bool _botLaunched;
        private bool _connectedToServer;
        private ILogger<MainWindowViewModel> _fileLogger;
        
        public IPanelLogger PanelLogger { get; set; }
        
        public ICommand StartBotCommand { get; }

        public ICommand OpenSettings { get; }
        
        public ICommand TestCommand { get; }

        public ForkMain CurrentActiveFork { get; set; } = new()
        {
            FirstBet = new() {Coefficient = 0},
            SecondBet = new() {Coefficient = 0}
        };

        public System.Timers.Timer Timer { get; set; } = new(1000);

        public StatisticModel Statistic { get; set; } = new();

        public string BotStatus { get; set; } = "Старт";

        public decimal FirstBookmakerStavka { get; set; } = 0;

        public decimal SecondBookmakerStavka { get; set; } = 0;

        public decimal firstBookmakerBalance = 0;

        public ForkSettings Settings = new();

        public decimal FirstBookmakerBalance
        {
            get => firstBookmakerBalance;
            set => firstBookmakerBalance = value;
        }

        public decimal secondBookmakerBalance = 0;
        public decimal SecondBookmakerBalance
        {
            get => secondBookmakerBalance;
            set => secondBookmakerBalance = value;
        }
        
        public float ForkProfit { get; set; }

        public MainWindowViewModel(
            ForksServiceProvider forksServiceProvider,
            BrowserOperations browserOperations,
            ForkBetOperations forkBetOperations,
            IPanelLogger panelLogger,
            IAbbForksProvider abbForksProvider,
            WindowManager windowManager, 
            IMediator mediator,
            ISettingsProvider settings,
            ILogger<MainWindowViewModel> fileLogger)
        {
            _forksProvider = forksServiceProvider;
            BrowserOperations = browserOperations;
            _forkBetOperations = forkBetOperations;
            _abbForksProvider = abbForksProvider;
            _mediator = mediator;
            PanelLogger = panelLogger;
            _fileLogger = fileLogger;
            _settingsProvider = settings;
            
            Timer.Elapsed += (sender, args) => TickTime();

            OpenSettings = new RelayCommand<object>(_ =>
            {
                windowManager.Show<BotSettingsWindow>();
            });
            StartBotCommand = new RelayCommand<object>(async _ =>
            {
                try
                {
                    if (_botLaunched || _botLaunching)
                    {
                        _botLaunching = false;
                        _botLaunched = !_botLaunched;
                        _token.Cancel();
                        await PanelLogger.AddInfoLog("Бот поставлен на паузу");
                        BotStatus = "Старт";
                        Timer.Stop();
                        LockBotProcessFork(true);
                    }
                    else
                    {
                        CurrentActiveFork = new();
                        _token = new CancellationTokenSource();
                        Statistic = new();
                        await PanelLogger.Clear();
                        _botLaunched = !_botLaunched;
                        _botLaunching = true;
                        FilterCountStakes.CountForksInOneEvent.Clear();
                        await PanelLogger.AddInfoLog("Бот запущен");
                        BotStatus = "Стоп";
                        Timer.Start();
                        var loginedInBrowsers = await BrowserOperations.LoginInBrowsers();;
                        if (!_connectedToServer && loginedInBrowsers){
                            await StartBot();
                            await PanelLogger.AddInfoLog("Подключился к серверу");
                            _connectedToServer = true;
                        }
                        if (!loginedInBrowsers)
                        {
                            await PanelLogger.AddInfoLog("Не смог залогиниться в букмекерках");
                        }
                        else
                        {
                            LockBotProcessFork(false);
                        }
                        Settings = settings.GetSettings();
                        ForkProfit = 0;
                    }
                }
                catch
                {
                    // ignored
                }
            });
            ViewModelUpdater.ForkProfit.Subscribe((value) =>
            {
                ForkProfit = value;
            });
            TestCommand = new RelayCommand<object>(async _ =>
            {
                var firstBet = new BetMain()
                {
                    Id = "",
                    Bookmaker = "fonbet",
                    EvId = "34612677",//Для открытия события на конторе
                    BetId = "34612677|921"//Для поиска и нажатия на ставку
                };
                var secondBet = new BetMain();
                var fork = new ForkMain()
                {
                    Away = "",
                    Elid = "",
                    Home = "",
                    Id = "",
                    K1 = "",
                    K2 = "",
                    Lifetime = 60,
                    Other = "",
                    Profit = 60m,
                    Sport = "",
                    FirstBet = firstBet,
                    SecondBet = secondBet,
                    ForkId = 0,
                    ForkScanner = ForkScannerType.AllBestBets,
                    SportId = 10,
                    UpdateCount = 0
                };
                //BetId(eventId,factorId) для поиска и нажатия на ставку
                var fonbetDecorator = BrowserOperations.Decorators.FirstOrDefault();
                var loadedEvent = await fonbetDecorator.LoadEvents(fork.FirstBet);
                var foundStake = await fonbetDecorator.FindStake(fork.FirstBet);
                var settedSum = await fonbetDecorator.SetStakeSum(30);
                var doStakeInfo = await fonbetDecorator.DoStake();
            });
        }

        private void TickTime()
        {
            Statistic.TimeOfCurrentSession += TimeSpan.FromSeconds(1);
            OnPropertyChanged(nameof(Statistic));
        }

        private async Task StartBot()
        {
            var connectedToAbb = await _mediator.Send(new StartBotCommand());
            if (!connectedToAbb)
            {
                await PanelLogger.AddInfoLog("Не смог подключиться к серверу");
                return;
            }
            _abbForksProvider.ForksReceived.Subscribe(AbbForkArrived);
        }
        
        private void AbbForkArrived(bool b)
        {
            if (_botProcessForkNow) return;
            
            LockBotProcessFork(true);
            ProcessFork();
        }

        private void LockBotProcessFork(bool b)
        {
            lock (SyncLock)
            {
                _botProcessForkNow = b;
            }
        }

        public bool ForkValid((bool checkCountForks,bool checkIdenticalStakes,bool checkNotOverlappedStakes,bool checkBetName) value)
        {
            return value.checkCountForks && value.checkIdenticalStakes && value.checkNotOverlappedStakes && value.checkBetName;
        }

        public async Task NotyfyUserForNotValidFork((bool checkCountForks,bool checkIdenticalStakes,bool checkNotOverlappedStakes,bool checkBetName) value)
        {
            if (!value.checkCountForks)
                await PanelLogger.AddInfoLog("Количество вилок в одном событии не пройден");

            if (!value.checkIdenticalStakes)
                await PanelLogger.AddInfoLog("Количество ставок в одном событии не пройден");

            if (!value.checkNotOverlappedStakes)
                await PanelLogger.AddInfoLog("Количество неперекрытых ставок не пройден");
                
            if (!value.checkBetName)
                await PanelLogger.AddInfoLog("Названия ставок не совпадают");
        }
        
        private async Task ProcessFork()
        {
            var fork = await _mediator.Send(new ForkQuery());

            // var checkForkResult =
            //     await _mediator.Send(new CheckForkCommand(fork), _token.Token);

            // if (!ForkValid(checkForkResult))
            // {
            //     NotyfyUserForNotValidFork(checkForkResult);
            //     fork = null;
            //     ++Statistic.CanceledForksByFilters;
            //     OnPropertyChanged(nameof(Statistic));
            // }

            if (fork != null)
            {
                try
                {
                    ++Statistic.ReceivedForks;
                    OnPropertyChanged(nameof(Statistic));
                    _token.Token.ThrowIfCancellationRequested();
                    fork.FirstBet.BetValue =  fork.FirstBet.BetValue.Replace("%s", fork.FirstBet.MarketAndBetTypeParam.ToString(CultureInfo.CurrentCulture));
                    fork.SecondBet.BetValue = fork.SecondBet.BetValue.Replace("%s", fork.SecondBet.MarketAndBetTypeParam.ToString(CultureInfo.CurrentCulture));
                    CurrentActiveFork = fork;
                    OnPropertyChanged(nameof(CurrentActiveFork));
                    ForkProfit = Convert.ToSingle(CurrentActiveFork.Profit);
                    OnPropertyChanged(nameof(ForkProfit));
                    var settingsForDynamicCheckStakesByCoefficient = _settingsProvider.GetSettings();
                    var (ok, firstBookmakerStavka, secondBookmakerStavka, notOverlappedForks) = await _mediator.Send(new ProcessForkCommand(CurrentActiveFork,
                        BrowserOperations.Decorators, Statistic.NotOverlappedForksCount,
                        firstBookmakerBalance, secondBookmakerBalance,settingsForDynamicCheckStakesByCoefficient), _token.Token);
                    
                    if (!ok)
                    {
                        OnPropertyChanged(nameof(Statistic));
                        await Task.Delay(Settings.PauseAfterUnsuccessfulAttemptPutDown * _thousand, _token.Token);
                    }
                    else
                    {
                        FirstBookmakerStavka = firstBookmakerStavka;
                        SecondBookmakerStavka = secondBookmakerStavka;
                        Statistic.NotOverlappedForksCount = notOverlappedForks;
                        ++Statistic.PuttedForksCount;
                        OnPropertyChanged(nameof(Statistic));
                        
                        var firstNameEvent = CurrentActiveFork.FirstBet.EventName;
                        var secondNameEvent = CurrentActiveFork.SecondBet.EventName;
                        var firstBetValue = CurrentActiveFork.FirstBet.BetValue;
                        var secondBetValue = CurrentActiveFork.SecondBet.BetValue;

                        FilterCountStakes.IncrementCountStakesInOneEvent(firstNameEvent, secondNameEvent, firstBetValue, secondBetValue);
                        FilterCountStakes.IncrementCountForksInOneEvent(firstNameEvent, secondNameEvent);

                        await Task.Delay(Settings.PauseAfterSuccessfulAttemptPutDown * _thousand, _token.Token);
                    }
                }
                catch
                {
                    // ignore
                }
            }
            
            await Task.Delay(3000, _token.Token);
            
            LockBotProcessFork(false);
            _abbForksProvider.ForksReceived.OnNext(true);
        }

        private async void HubForksArrived(PositiveFork fork)
        {
            using var releaser = await Lock.LockAsync();
            if (fork != null)
            {
                CurrentActiveFork = fork.Convert();
                await Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    await PanelLogger.AddForkInfo("Пришла вилка", "", fork.Teams, fork.Bookmakers,
                        fork.Sport.ToString()));
                
                await _forkBetOperations.CheckBets(fork.Convert(),  BrowserOperations.Decorators);
            }

            await _forksProvider.ReadyToGetForksSafe();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}