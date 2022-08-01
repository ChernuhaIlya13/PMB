using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using PMB.Application;
using PMB.Application.Interfaces;
using PMB.Browsers;
using PMB.Browsers.Common;
using PMB.Browsers.Common.Exceptions;
using PMB.Cef.Core;
using PMB.Domain.Logger;
using PMB.Utils;
using PMB.Wpf.Client.Infra;
using PMB.Wpf.Client.Utilities;

namespace PMB.Wpf.Client.Operations;

public class BrowserOperations
{
    private readonly DecoratorFactory _decoratorFactory;
    private readonly IPanelLogger _customLogger;
    private readonly ISettingsProvider _settingsProvider;
        
    public readonly List<IBotBrowserDecorator> Decorators = new();

    public BrowserOperations(IPanelLogger customLogger, ISettingsProvider settingsProvider, DecoratorFactory decoratorFactory)
    {
        _customLogger = customLogger;
        _settingsProvider = settingsProvider;
        _decoratorFactory = decoratorFactory;

        ViewModelUpdater.BrowserRemove.Subscribe((bro) =>
        {
            var decoratorForRemove = Decorators.FirstOrDefault(d => d.BookmakerName == bro);
            decoratorForRemove?.CloseBrowser();
            Decorators.Remove(decoratorForRemove);
        });
    }

    public async Task CreateBookmaker(string bookmakerName)
    {
        try
        {
            var settings = _settingsProvider.GetSettings();
            var bookmakerSettings = settings.Bookmakers.FirstOrDefault(b => b.BookmakerName == bookmakerName);
            var decorator = _decoratorFactory.ResolveDecorator(bookmakerSettings);
            var _ = await decorator.CreateBrowser(bookmakerSettings, _customLogger, CancellationToken.None);
            Decorators.Add(decorator);
        }
        catch
        {
            //ignore
        }
    }

    public async Task<bool> LoginInBrowsers()
    {
        var settings = _settingsProvider.GetSettings();
        var loginTaskResult = Decorators.Select(async d =>
        {
            var settingsForCurrentBrowser = settings.Bookmakers.FirstOrDefault(b => b.BookmakerName == d.BookmakerName);
            var logined = false;
            if (settingsForCurrentBrowser.BrowserOptions.Auth.UseAuthInBk)
            {
                logined = await d.Login(settingsForCurrentBrowser, CancellationToken.None);
            }
            return logined;
        }).ToArray();
        var result = await Task.WhenAll(loginTaskResult);
        return result.All(r => r);
    }
    
    public async Task CreateAllBrowsersWithoutLogin(CancellationToken token)
    {
        try
        {
            var settings = _settingsProvider.GetSettings();

            var result = settings.Bookmakers.Where(x => x.IsActive).Select(async b =>
            {
                var decorator = _decoratorFactory.ResolveDecorator(b);
                var _ = await decorator.CreateBrowser(b, _customLogger, token);
                Decorators.Add(decorator);
            });
            await Task.WhenAll(result);
        }
        catch
        {
            
        }

    }

    public async Task<bool> CreateAllBrowsers(CancellationToken token)
    {
        var settings = _settingsProvider.GetSettings();
            
        var browserTasks = settings.Bookmakers.Where(x => x.IsActive).Select(async b =>
        {
            var logined = false;
            try
            {
                var decorator = _decoratorFactory.ResolveDecorator(b);
                var _ = await decorator.CreateBrowser(b, _customLogger, token);
                Decorators.Add(decorator);
                if (b.BrowserOptions.Auth.UseAuthInBk)
                {
                    logined = await decorator.Login(b, token);
                }
            }
            catch (BookmakerNotFoundException)
            {
                // TODO
                // ignore
            }
            return logined;
        }).ToArray();
            
        var result = await Task.WhenAll(browserTasks);
        return result.All(r => r);
    }
        
    public async Task<bool> DoStakeAndCheckWindowResult(BotBrowser browser)
    {
        await browser.Worker.DoStake();
        await browser.Worker.CheckWindowResult();
        var ctsToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
        try
        {
            await browser.Worker.CouponWindowResult
                .FirstAsync(x => x.Contains(browser.BookmakerName.Substring(1))).ToTask(ctsToken.Token);
            var windowResultTextSuccess = await browser.Worker.CheckWindowResultText();
            if (!windowResultTextSuccess)
            {
                await _customLogger.AddInfoLog("Не удалось сделать ставку на " + browser.BookmakerName);
                return false;
            }
        }
        catch (Exception)
        {
            await _customLogger.AddInfoLog("Не удалось сделать ставку на " + browser.BookmakerName);
            return false;
        }

        return true;
    }
        
    public bool ProcessBrowserTask(ResponseFromBrowser response)
    {
        bool PrintWithFalse()
        {
            PrintInLogInfo(response);
            return false;
        }
            
        return response.StatusStake switch
        {
            ResponseStatusStake.NotFoundStake => PrintWithFalse(),
            ResponseStatusStake.FailedSetStakeSum => PrintWithFalse(),
            ResponseStatusStake.Success => true,
            _ => false
        };
    }

    private async Task PrintInLogInfo(ResponseFromBrowser response)
    {
        await _customLogger.AddInfoLog($"{response?.StatusStake.Convert()} {response?.BrowserName}");
    }
}