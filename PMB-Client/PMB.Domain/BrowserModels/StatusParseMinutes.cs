namespace PMB.Domain.BrowserModels
{
    public enum StatusParseMinutes
    {
        None,
        TimeFailedParse,//не удалось спарсить ставку
        TimeMatch,//время совпадает
        TimeNotMatch,//время не совпадаеат
        UnknownFail,//неизвестная ошибка
    }
}