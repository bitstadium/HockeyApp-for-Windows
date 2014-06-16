using Caliburn.Micro;
using HockeyApp.AppLoader.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
        

        public static async Task<ProgressDialogController> ShowProgressAsync(this IWindowManager windowManager, string title, string message, bool isCancelable=false, MetroDialogSettings settings = null){
            MetroWindow mw = Application.Current.MainWindow as MetroWindow;
            return  await mw.ShowProgressAsync(title, message, isCancelable, settings);
        }

        public static async Task<MessageDialogResult> ShowMessageAsync(this IWindowManager @this, string title, string message, MessageDialogStyle style, MetroDialogSettings settings)
        {
            MetroWindow mw = Application.Current.MainWindow as MetroWindow;
            return await mw.ShowMessageAsync(title, message, style, settings);
        }

        public async static Task<MessageDialogResult> ShowSimpleMessageAsync(this IWindowManager @this, string title, string message)
        {
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK"
            };
            return await @this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, settings);
        }
    }
}
