using HockeyApp.AppLoader.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HockeyApp.AppLoader.Views
{
    /// <summary>
    /// Interaction logic for LookupApiTokenView.xaml
    /// </summary>
    public partial class LookupApiTokenView : MetroWindow
    {
        public LookupApiTokenView()
        {
            InitializeComponent();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LookupApiTokenViewModel vm = this.DataContext as LookupApiTokenViewModel;
            if (vm != null)
            {
                vm.Password = txtPassword.Password;
            }
        }

        public void HandleListViewDoubleClick(object sender, object args)
        {
            LookupApiTokenViewModel vm = this.DataContext as LookupApiTokenViewModel;
            if (vm != null && vm.CanOK)
            {
                vm.OK();
            }
        }


        private void lvApiTokens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvApiTokens.Items.Count > 0)
            {
                object item = this.lvApiTokens.Items[this.lvApiTokens.Items.Count - 1];
                var element = this.lvApiTokens.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (element != null)
                {
                    element.BringIntoView();
                }
            }
        }
    }
}
