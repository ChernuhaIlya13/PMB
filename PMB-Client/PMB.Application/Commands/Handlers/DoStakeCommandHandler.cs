using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CefSharp.Wpf;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Application.Models;
using PMB.Domain.BrowserModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record DoStakeCommand(BetDecorator[] BetDecorators, int NotOverlappedForks): IRequest<(bool Ok, int NotOverlappedForks)>;

[UsedImplicitly]
internal sealed class DoStakeCommandHandler: IRequestHandler<DoStakeCommand, (bool Ok, int NotOverlappedForks)>
{
    private readonly IPanelLogger _panelLogger;
    private readonly ForkSettings _forkSettings;
    
    public DoStakeCommandHandler(IPanelLogger panelLogger, ISettingsProvider settingsProvider)
    {
        _panelLogger = panelLogger;
        _forkSettings = settingsProvider.GetSettings();
    }

    public async Task<(bool Ok, int NotOverlappedForks)> Handle(DoStakeCommand request, CancellationToken cancellationToken)
    {
        var firstBkEventName = request.BetDecorators.First().BetMain.EventName;
        var secondBkEventName = request.BetDecorators.Skip(1).First().BetMain.EventName;
        
        var maxMinusForFork = _forkSettings.MaxMinus;
        if (maxMinusForFork > 0)
            maxMinusForFork *= -1;
        
        var timer = new Stopwatch();

        var puttedStakeCoefs = new List<decimal>();

        var notOverlappedForks = request.NotOverlappedForks;
        
        for (int i = 0; i < request.BetDecorators.Length; i++)
        {
            var (bet,botBrowserDecorator) = request.BetDecorators[i];
            var isStaked = false;
            var coefficient = 0m;
            
            if (i == 0)
            {
                try
                {
                    (isStaked, coefficient) = await botBrowserDecorator.DoStakeWithLoginCheck();
                }
                catch
                {
                    isStaked = false;
                }
            }

            if (i == 1)
            { 
                timer.Start();

                while (!isStaked && timer.Elapsed < TimeSpan.FromSeconds(_forkSettings.WaitingShoulderOverlap))
                {

                    var coefFromCoupon = 0m;
                    try
                    {
                        coefFromCoupon = await botBrowserDecorator.GetStakeCoefficientFromCoupon(bet);
                    }
                    catch(Exception ex)
                    {
                        // ignore
                    }
                    var currentForkProfit = CalcCurrentForkProfit(puttedStakeCoefs.First(), coefFromCoupon);
                
                    if (currentForkProfit >= maxMinusForFork)
                    {
                        try
                        {
                            (isStaked, _) = await botBrowserDecorator.DoStakeWithLoginCheck();
                        }
                        catch
                        {
                            isStaked = false;
                        }
                    }
                    
                    await Task.Delay(1000, cancellationToken);
                }
                timer.Stop();
            }
            /*TODO:РАскоментить после теста Фонбета
            if (!isStaked)
            {
                await _panelLogger.AddInfoLog($"Не удалось проставить ставку на {botBrowserDecorator.BookmakerName}");
                if (i == 1)
                {
                    FilterCountStakes.IncrementCountNotOverlappedStakes(firstBkEventName,secondBkEventName);
                    notOverlappedForks += 1;
                }
                return (false, notOverlappedForks);
            }
*/
            var valuePuttedStake = 0m;
            try
            {
                valuePuttedStake = await botBrowserDecorator.GetPuttedCoefStake();
            }
            catch
            {
                // ignored
            }

            puttedStakeCoefs.Add(valuePuttedStake);
            
            await _panelLogger.AddInfoLog($"Ставка успешно проставлена на {botBrowserDecorator.BookmakerName}");
        }
        
        return (true, notOverlappedForks);
    }

    public static decimal CalcCurrentForkProfit(decimal firstCoef, decimal secondCoef) => firstCoef != 0 && secondCoef != 0
        ? (1m - Math.Round(1 / firstCoef + 1 / secondCoef, 2)) * 100
        : 0;
}