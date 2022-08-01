using System;
using System.Linq;
using PMB.Application.Interfaces;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Wpf.Client.Services
{
    public class ForkProcessor
    {
        private ISettingsProvider _settingsProvider; 
        public ForkProcessor(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }
        public bool Handle(ForkMain fork, ForkSettings settings)
        {
            var allBookmakers = settings?.Bookmakers.Select(b => b.BookmakerName.Replace("Ru","")).ToArray();

            return allBookmakers!.Any(
                       book => fork.FirstBet.Bookmaker.Contains(book, StringComparison.OrdinalIgnoreCase)) &&
                   allBookmakers.Any(book =>
                       fork.SecondBet.Bookmaker.Contains(book, StringComparison.OrdinalIgnoreCase)) &&
                   settings!.TimeOfLife.Start <= fork.Lifetime && settings.TimeOfLife.Finish >= fork.Lifetime &&
                   settings.Profit.Start <= fork.Profit && settings.Profit.Finish >= fork.Profit &&
                   settings.Coefficient.Start <= fork.FirstBet.Coefficient &&
                   settings.Coefficient.Start <= fork.SecondBet.Coefficient &&
                   settings.Coefficient.Finish >= fork.FirstBet.Coefficient &&
                   settings.Coefficient.Finish >= fork.SecondBet.Coefficient;
        }
    }
}