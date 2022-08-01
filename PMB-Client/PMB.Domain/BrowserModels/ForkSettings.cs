using System.Collections.Generic;
using System.Collections.ObjectModel;
using PMB.Domain.ForkModels;

namespace PMB.Domain.BrowserModels
{
    public class ForkSettings
    {
        /// <summary>
        /// Пауза после успешной попытки проставить
        /// </summary>
        public int PauseAfterSuccessfulAttemptPutDown { get; set; }

        /// <summary>
        /// Пауза после неудачной попытки проставить
        /// </summary>
        public int PauseAfterUnsuccessfulAttemptPutDown { get; set; }

        /// <summary>
        /// ожидание перекрытия плеча
        /// </summary>
        public int WaitingShoulderOverlap { get; set; }

        /// <summary>
        /// Максимальный минус
        /// </summary>
        public int MaxMinus { get; set; }

        /// <summary>
        /// Количество вилок в одном событии 
        /// </summary>
        public int CountForksInOneEvent { get; set; }

        /// <summary>
        /// количество идентичных вилок в одном событии
        /// </summary>
        public int CountOfIdenticalSurebetsInOneEvent { get; set; }

        /// <summary>
        /// количество допустимых неперекрытых в одном событии
        /// </summary>
        public int CountAdmissibleNonOverlappedInOneEvent { get; set; }

        /// <summary>
        /// Доходность вилки
        /// </summary>
        public Range Profit { get; set; } = new();

        /// <summary>
        /// Коэффициент вилки
        /// </summary>
        public Range Coefficient { get; set; } = new();

        /// <summary>
        /// Время жизни вилки
        /// </summary>
        public Range TimeOfLife { get; set; } = new();

        /// <summary>
        /// Умная проставка
        /// </summary>
        public SmartSpacer CleverStake { get; set; } = new();

        /// <summary>
        /// Пропустить ставку,если максимально возможная ставка в конторе меньше чем ...
        /// </summary>
        public int MaxSumStakeInBk { get; set; }

        /// <summary>
        /// Выбирается случайное число в диапозоне умной проставки
        /// Либо процент от лимита на ставку,либо процент от баланса на БК
        /// </summary>

        /// <summary>
        /// Список букмекеров
        /// </summary>

        public List<Bookmaker> Bookmakers { get; set; } = new();

        ///
        /// Правила очередности
        ///
        public ObservableCollection<SequenceBookmakers> SequenceRulesBookmakers { get; set; } = new();

        /// <summary>
        /// Проверка коэффициентов ставок
        /// </summary>
        public bool CheckByCoefficientStakes { get; set; } = false;
        
        public class SmartSpacer : Range
        {
            public decimal PercentLimitOnStake { get; set; }
        }

        public class SequenceBookmakers
        {
            public int Id { get; set; }

            public string First { get; set; }

            public string Second { get; set; }
        }
    }

}
