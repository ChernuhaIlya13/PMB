using System;
using System.Collections.Generic;
using System.Linq;
using PMB.Browsers.Common;
using PMB.Browsers.Common.Exceptions;
using PMB.Domain.BrowserModels;

namespace PMB.Browsers
{
    public class DecoratorFactory
    {
        private readonly IEnumerable<IBotBrowserDecorator> _browserDecorators;

        public DecoratorFactory(IEnumerable<IBotBrowserDecorator> browserDecorators)
        {
            _browserDecorators = browserDecorators;
        }

        public IBotBrowserDecorator ResolveDecorator(Bookmaker bookmaker)
        {
            var decorator = _browserDecorators.FirstOrDefault(x =>
                x.BookmakerName.Equals(bookmaker.BookmakerName, StringComparison.InvariantCultureIgnoreCase));

            if (decorator == null)
            {
                throw new BookmakerNotFoundException(bookmaker.BookmakerName);
            }
            
            return decorator;
        }
    }
}