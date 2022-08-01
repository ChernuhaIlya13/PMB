namespace PMB.Application.Models;

public enum StakeSetStatus
{
    Success,
    UnhandledException,
    NotFoundStake,
    FailedSetStakeSum,
    BalanceTrouble,
    NotEnoughMoneyForCurrentStake,
    FailedGetMinMaxStake
}