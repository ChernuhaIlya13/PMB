using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace PMB.Cef.Core.Handlers
{
    public class CustomContextMenuHandler:IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters,
            IMenuModel model)
        {
        }

        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters,
            CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public bool RunContextMenu(IWebBrowser webIBro, IBrowser browser, IFrame frame, IContextMenuParams parameters,
            IMenuModel model, IRunContextMenuCallback callback)
        {
             ChromiumWebBrowser chromiumWebBrowser = (ChromiumWebBrowser)webIBro;
            chromiumWebBrowser.Dispatcher.Invoke((Action)(() =>
            {
                ContextMenu menu = new ContextMenu()
                {
                    IsOpen = true
                };
                menu.Closed += new RoutedEventHandler(Handler);
                
                ItemCollection items1 = menu.Items;
                var tools = new MenuItem()
                {
                    Header = ((object)"Инструменты разработчика")
                };
                tools.Click += (RoutedEventHandler)((_param1, _param2) => chromiumWebBrowser.ShowDevTools());
                items1.Add((object) tools);
                items1.Add((object)new MenuItem()
                {
                    Header = (object)"Назад",
                    Command = chromiumWebBrowser.BackCommand
                });
                ItemCollection items2 = menu.Items;
                items2.Add((object)new MenuItem()
                {
                    Header = (object)"Вперед",
                    Command = chromiumWebBrowser.ForwardCommand
                });
                ItemCollection items3 = menu.Items;
                items3.Add((object)new MenuItem()
                {
                    Header = (object)"Обновить",
                    Command = chromiumWebBrowser.ReloadCommand
                });
                menu.Items.Add((object)new Separator());
                ItemCollection items4 = menu.Items;
                items4.Add((object)new MenuItem()
                {
                    Header = (object)"Копировать",
                    Command = chromiumWebBrowser.CopyCommand
                });
                ItemCollection items5 = menu.Items;
                items5.Add((object)new MenuItem()
                {
                    Header = (object)"Вставить",
                    Command = chromiumWebBrowser.PasteCommand
                });
                menu.Items.Add((object)new Separator());
                
                chromiumWebBrowser.ContextMenu = menu;

                void Handler(object s, RoutedEventArgs e)
                {
                    // ISSUE: method pointer
                    //menu.Closed -= new RoutedEventHandler((object) this, __methodptr(\u003CRunContextMenu\u003Eg__Handler\u007C1));
                    if (callback.IsDisposed)
                        return;
                    callback.Cancel();
                }
            }));
            return true;
        }
    }
}