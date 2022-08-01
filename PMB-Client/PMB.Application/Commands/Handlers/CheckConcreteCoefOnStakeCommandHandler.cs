using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PMB.Application.Models;

namespace PMB.Application.Commands.Handlers;

public record CheckConcreteCoefStakeCommand(BetDecorator[] BetDecorators) : IRequest<bool>;

internal sealed class CheckConcreteCoefOnStakeCommandHandler : IRequestHandler<CheckConcreteCoefStakeCommand,bool>
{
    public async Task<bool> Handle(CheckConcreteCoefStakeCommand request, CancellationToken cancellationToken)
    {
        var firstBetDecorator = request.BetDecorators.First();
        var firstBet = request.BetDecorators.First().BetMain;
        var secondBetDecorator = request.BetDecorators.Skip(1).First();
        var secondBet = request.BetDecorators.Skip(1).First().BetMain;
        bool secondHomeParam = !(firstBet.SwapTeams || secondBet.SwapTeams);
        var firstStakesCoefs = (0d,0d);
        var secondStakesCoefs = (0d,0d);
        
        //Получить ставки с контор
        //Найти общую ставку
        //Вычислить разницу
        
        try
        {
            firstStakesCoefs = await firstBetDecorator.Decorator.GetStakesCoefs(firstBetDecorator.BetMain, secondBetDecorator.BetMain);
            secondStakesCoefs = await secondBetDecorator.Decorator.GetStakesCoefs(secondBetDecorator.BetMain, firstBetDecorator.BetMain);
        }
        catch
        {
            return false;
        }
        //Разность между ставками должна быть равна не больше 0.5
        if (Math.Abs(firstStakesCoefs.Item1 - secondStakesCoefs.Item1) <= 0.5 &&
            Math.Abs(firstStakesCoefs.Item2 - Math.Abs(secondStakesCoefs.Item2)) <= 0.5)
        {
            return true;
        }

        return false;
    }
}