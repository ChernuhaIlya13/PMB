using PMB.Domain.ForkModels;

namespace PMB.Domain.BrowserModels
{
    public class Bookmaker
    {
        /// <summary>
        /// Выбран ли букмекер пользователем
        /// </summary>
        public bool IsActive { get; set; } = false;
        
        public BrowserOptions BrowserOptions { get; set; } = new();
        /// <summary>
        /// Имя букмекера
        /// </summary>
        public string BookmakerName { get; set; }

        /// <summary>
        /// Адрес зеркала
        /// </summary>
        public string Uri { get; set; }
        
        /// <summary>
        /// Валюта
        /// </summary>
        public Currency CurrencyInfo { get; set; } = new();
        
        /// <summary>
        /// Прибыль вилки
        /// </summary>
        public ForkInfo ProfitFork { get; set; } = new();
        
        /// <summary>
        /// Коэффициент вилки
        /// </summary>
        public ForkInfo CoefficientFork { get; set; } = new();
        
        /// <summary>
        /// Время жизни вилки
        /// </summary>
        public ForkInfo TimeOfLifeFork { get; set; }= new();
        
        /// <summary>
        /// Пауза после успешной попытки проставить
        /// </summary>
        public ForkInfo PauseAfterSuccessfulAttemptPutDown { get; set; } = new();
        
        /// <summary>
        /// Количество вилок в одном событии 
        /// </summary>
        public ForksDetails CountForksInOneEvent { get; set; } = new();
        
        /// <summary>
        /// количество идентичных вилок в одном событии
        /// </summary>
        public ForksDetails CountOfIdenticalSurebetsInOneEvent { get; set; } = new();
        
        /// <summary>
        /// Остановка бота при балансе на бк
        /// </summary>
        public Range StopBotBalance { get; set; } = new();
        
        /// <summary>
        /// Использовать ограничения сумм
        /// </summary>
        public ForkInfo RestrictSum { get; set; } = new();
        
        /// <summary>
        /// Проверка минимумов и максимумов в букмекерке
        /// </summary>
        public bool CheckMaxMin { get; set; }
        
        /// <summary>
        /// Ограничение инициатора
        /// </summary>
        public string RestrictInitiator { get; set; }
        
        /// <summary>
        /// Умное замедление
        /// </summary>
        public bool CleverSlowing { get; set; }

        public class Currency {
            public string CurrencyName { get; set; }
            public string RoundRule { get; set; }
            public bool UseCommonRuleRound { get; set; }
        }

        public class ForkInfo : Range
        {
            public bool UseCommonRuleRound { get; set; }
        }

        public class ForksDetails
        {
            public int CountForks { get; set; } 
            public bool UseCommonRuleRound { get; set; }
        }
    }
}