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
    /// Interaction logic for AddUserConfiguration.xaml
    /// </summary>
    public partial class AddUserConfigurationView : UserControl
    {
        public AddUserConfigurationView()
        {
            InitializeComponent();
            this.Loaded += AddUserConfigurationView_Loaded;
        }

        void AddUserConfigurationView_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtConfigurationName.Focus();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            AddUserConfigurationViewModel vm = this.DataContext as AddUserConfigurationViewModel;
            if (vm != null)
            {
                vm.Password = this.txtPassword.Password;
            }
        }

        private void lvApiTokens_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddUserConfigurationViewModel vm = this.DataContext as AddUserConfigurationViewModel;
            if (vm != null)
            {
                vm.OK();
            }
        }

        private async void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            AddUserConfigurationViewModel vm = this.DataContext as AddUserConfigurationViewModel;
            if (e.Key == Key.Enter && vm != null && vm.CanLogin)
            {
                await vm.Login();
            }
        }

        


    }
}
