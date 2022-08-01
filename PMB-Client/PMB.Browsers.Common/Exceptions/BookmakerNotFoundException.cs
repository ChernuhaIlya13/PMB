using System;

namespace PMB.Browsers.Common.Exceptions
{
    public class BookmakerNotFoundException: Exception
    {
        public BookmakerNotFoundException(string bookmakerName): base($"Реализация для {bookmakerName} не найдена")
        { }
    }
}