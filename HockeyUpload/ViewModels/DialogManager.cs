using Caliburn.Micro;
using HockeyApp.AppLoader.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp.AppLoader.ViewModels
{
    public static class DialogManager
    {
        private static MetroWindow GetMainView(){
            MainWindowViewModel vm = IoC.Get<MainWindowViewModel>();
            return Application.Current.MainWindow as MetroWindow;
        }
        
        public static Task<ProgressDialogController> ShowProgressAsync(string title, string message, bool isCancelable = false, MetroDialogSettings settings = null)
        {
            MetroWindow window = GetMainView();
            window.Dispatcher.VerifyAccess();

            return window.ShowOverlayAsync().ContinueWith(z =>
            {
                return ((Task<ProgressDialogController>)window.Dispatcher.Invoke(new Func<Task<ProgressDialogController>>(() =>
                {
                    //create the dialog control
                    ProgressDialog dialog = new ProgressDialog(window);
                    dialog.Message = message;
                    dialog.Title = title;
                    dialog.IsCancelable = isCancelable;

                    if (settings == null)
                        settings = window.MetroDialogOptions;

                    dialog.NegativeButtonText = settings.NegativeButtonText;

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                    {
                        if (DialogOpened != null)
                        {
                            window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs()
                            {
                            })));
                        }

                        return new ProgressDialogController(dialog, () =>
                        {
                            dialog.OnClose();

                            if (DialogClosed != null)
                            {
                                window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs()
                                {
                                })));
                            }

                            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog._WaitForCloseAsync()));
                            return closingTask.ContinueWith<Task>(a =>
                            {
                                return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                {
                                    window.SizeChanged -= sizeHandler;

                                    window.messageDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                                    return window.HideOverlayAsync();
                                    //window.overlayBox.Visibility = System.Windows.Visibility.Hidden; //deactive the overlay effect
                                }));
                            }).Unwrap();
                        });
                    });
                })));
            }).Unwrap();
        }
        
    }
}
