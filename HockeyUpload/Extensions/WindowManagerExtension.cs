using Caliburn.Micro;
using HockeyApp.AppLoader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp.AppLoader.Extensions
{
    public static class WindowManagerExtensions
    {
        public static MessageBoxResult ShowMetroMessageBox(this IWindowManager @this, string message, string title, MessageBoxButton buttons)
        {
            MessageBoxResult retval;
            var shellViewModel = Caliburn.Micro.IoC.Get<MainWindowViewModel>();

            try
            {
                shellViewModel.ShowOverlay();
                var model = new MetroMessageBoxViewModel(message, title, buttons);
                @this.ShowDialog(model);

                retval = model.Result;
            }
            finally
            {
                shellViewModel.HideOverlay();
            }

            return retval;
        }

        public static void ShowMetroMessageBox(this IWindowManager @this, string message)
        {
            @this.ShowMetroMessageBox(message, "System Message", MessageBoxButton.OK);
        }

        public static void ShowBusyView(this IWindowManager @this, string busyMessage="")
        {
            var shellViewModel = Caliburn.Micro.IoC.Get<MainWindowViewModel>();
            shellViewModel.BusyMessage = busyMessage;
            shellViewModel.ViewIsBusy = true;
        }

        public static void HideBusyView(this IWindowManager @this)
        {
            var shellViewModel = Caliburn.Micro.IoC.Get<MainWindowViewModel>();
            shellViewModel.ViewIsBusy = false;
        }
    }
}
