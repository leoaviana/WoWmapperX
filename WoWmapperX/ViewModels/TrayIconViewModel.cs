using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using ReactiveUI;
using WoWmapperX.Views;

namespace WoWmapperX.ViewModels
{
    public class TrayIconViewModel : ReactiveObject
    {
        public TrayIconViewModel()
        {
            ShowCommand = ReactiveCommand.Create(ShowMainWindow);
            ExitCommand = ReactiveCommand.Create(ExitApplication);
        }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ShowCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ExitCommand { get; }

        private void ShowMainWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow.Instance.Show();
            }
        }

        private void ExitApplication()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}
