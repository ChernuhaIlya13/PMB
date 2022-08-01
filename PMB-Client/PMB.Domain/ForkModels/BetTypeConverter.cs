using PMB.Abb.Models.Models;

namespace PMB.Domain.ForkModels
{
    public static class BetTypeConverter
    {
        //TODO:Дописать парсинг типов ставок(не срочно)
        public static string ConvertToBetType(this string value)
        {
            return AbbBetType.Cards.ToString();
        } 
    }
}