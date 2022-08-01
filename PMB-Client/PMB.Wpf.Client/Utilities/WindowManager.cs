using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PMB.Wpf.Client.Utilities;

public class WindowManager
{
    private readonly IServiceProvider _serviceProvider;

    public WindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public WindowManager Show<T>() where T : Window
    {
        var service = _serviceProvider.GetRequiredService<T>();
        service.Show();
        service.Activate();
        return this;
    }
}