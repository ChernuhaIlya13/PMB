using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Application.Models;
using PMB.Browsers.Common;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Queries.Handlers;

internal record BetDecoratorsQuery(BetMain[] BetsFromStash, IBotBrowserDecorator[] Decorators) : IRequest<BetDecoratorsResult>;

internal record BetDecoratorsResult(BetDecorator[] BetDecorators);

[UsedImplicitly]
internal sealed class BetDecoratorsQueryHandler : IRequestHandler<BetDecoratorsQuery, BetDecoratorsResult>
{
    private readonly IPanelLogger _panelLogger;
    private readonly ForkSettings _forkSettings;
    
    public BetDecoratorsQueryHandler(IPanelLogger panelLogger, ISettingsProvider settingsProvider)
    {
        _panelLogger = panelLogger;
        _forkSettings = settingsProvider.GetSettings();
    }

    public async Task<BetDecoratorsResult> Handle(BetDecoratorsQuery request, CancellationToken cancellationToken)
    {
        var betsFromStash = request.BetsFromStash;
        var decorators = request.Decorators;
        
        var betDecorators = await GetDecoratorsForCurrentFork(decorators.ToList(), betsFromStash);
        if (betDecorators?.Any() != true)
            return null;
        
        //betDecorators.Reverse();

        var bookmakerNames = betDecorators.Select(x => x.Decorator.BookmakerName).ToArray();

        var restrictions = _forkSettings.SequenceRulesBookmakers.Where(x =>
            bookmakerNames.Contains(x.First) && bookmakerNames.Contains(x.Second));

        foreach (var restriction in restrictions)
        {
            var first = betDecorators.First(x =>
                x.Decorator.BookmakerName.Equals(restriction.First, StringComparison.InvariantCultureIgnoreCase));

            var second = betDecorators.First(x =>
                x.Decorator.BookmakerName.Equals(restriction.Second, StringComparison.InvariantCultureIgnoreCase));

            betDecorators = new List<BetDecorator>
            {
                first, second
            };
        }

        return new BetDecoratorsResult(betDecorators.ToArray());
    }
    
    [ItemCanBeNull]
    private async Task<List<BetDecorator>> GetDecoratorsForCurrentFork(
        List<IBotBrowserDecorator> decorators,
        IReadOnlyCollection<BetMain> bets)
    {
        var betDecoratorPairs = new List<BetDecorator>();
        
        decorators.ForEach(decorator =>
        {
            //TODO:Что это [1..],надо чекнуть
            var bet = bets.FirstOrDefault(bet =>
                decorator.BookmakerName.Contains(bet.Bookmaker[1..], StringComparison.OrdinalIgnoreCase));

            if (bet != null)
            {
                betDecoratorPairs.Add(new(bet, decorator));
            }
        });

            if (betDecoratorPairs.Count < 2)
        {
            await _panelLogger.AddInfoLog("2 Букмекера минимум должны быть активны. Поменяйте настройки");
            return null;
        }

        return betDecoratorPairs;
    }
}