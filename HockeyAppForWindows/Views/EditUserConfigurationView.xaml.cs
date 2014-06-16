using HockeyApp.AppLoader.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HockeyApp.AppLoader.Views
{
    /// <summary>
    /// Interaction logic for EditUserConfigurationView.xaml
    /// </summary>
    public partial class EditUserConfigurationView : UserControl
    {
        public EditUserConfigurationView()
        {
            InitializeComponent();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EditUserConfigurationViewModel vm = this.DataContext as EditUserConfigurationViewModel;
            if (vm != null)
            {
                vm.Password = this.txtPassword.Password;
            }
        }

        private void lvApiTokens_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditUserConfigurationViewModel vm = this.DataContext as EditUserConfigurationViewModel;
            if (vm != null)
            {
                vm.OK();
            }
        }

        private async void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            EditUserConfigurationViewModel vm = this.DataContext as EditUserConfigurationViewModel;
            if (e.Key == Key.Enter && vm != null && vm.CanLogin)
            {
                await vm.Login();
            }
        }
    }
}
