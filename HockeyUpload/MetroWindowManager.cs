using Caliburn.Micro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HockeyApp.AppLoader
{
    public class MetroWindowManager : WindowManager
    {
        private ResourceDictionary[] _resourceDictionaries;

        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            MetroWindow window = null;
            Window inferOwnerOf;
            if (view is MetroWindow)
            {
                window = CreateCustomWindow(view, true);
                inferOwnerOf = InferOwnerOf(window);
                if (inferOwnerOf != null && isDialog)
                {
                    window.Owner = inferOwnerOf;
                }
            }

            if (window == null)
            {
                window = CreateCustomWindow(view, false);
            }

            ConfigureWindow(window);
            window.SetValue(Caliburn.Micro.View.IsGeneratedProperty, true);
            inferOwnerOf = InferOwnerOf(window);
            if (inferOwnerOf != null)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = inferOwnerOf;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            return window;
        }

        public virtual void ConfigureWindow(MetroWindow window)
        {

        }
        public virtual MetroWindow CreateCustomWindow(object view, bool windowIsView)
        {
            MetroWindow result;
            if (windowIsView)
            {
                result = view as MetroWindow;
            }
            else
            {
                result = new MetroWindow
                {
                    Content = view
                };
            }

            AddMetroResources(result);
            return result;
        }

        private void AddMetroResources(MetroWindow window)
        {
            _resourceDictionaries = LoadResources();
            foreach (ResourceDictionary dictionary in _resourceDictionaries)
            {
                window.Resources.MergedDictionaries.Add(dictionary);
            }
        }

        private ResourceDictionary[] LoadResources()
        {
            return new[]
                       { 
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                               /*
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Colours.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                                */
                               new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/Views/MyColors.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Controls.AnimatedSingleRowTabControl.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/FlatButton.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml",
                                       UriKind.RelativeOrAbsolute)
                               }
                       };
        }
    }
}
