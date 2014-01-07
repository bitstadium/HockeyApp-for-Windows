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
    /// Interaction logic for MetroMessageBoxView.xaml
    /// </summary>
    public partial class MetroMessageBoxView : MetroWindow
    {
        public MetroMessageBoxView()
        {
            InitializeComponent();
        }


        protected override void OnActivated(EventArgs e)
        {
            if (Owner != null)
            {
                if (Owner.WindowState == WindowState.Maximized)
                {
                    Left = 0;
                    Top = (Owner.Height - 200) / 2;
                    Width = Owner.Width;
                }
                else
                {
                    Left = Owner.Left + 1;
                    Top = Owner.Top + ((Owner.Height - 200) / 2);
                    Width = Owner.Width - 2;
                }
            }

            base.OnActivated(e);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            MetroMessageBoxViewModel vm = this.DataContext as MetroMessageBoxViewModel;
            if (vm != null)
            {
                vm.Yes();
            }
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            MetroMessageBoxViewModel vm = this.DataContext as MetroMessageBoxViewModel;
            if (vm != null)
            {
                vm.No();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            MetroMessageBoxViewModel vm = this.DataContext as MetroMessageBoxViewModel;
            if (vm != null)
            {
                vm.Ok();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MetroMessageBoxViewModel vm = this.DataContext as MetroMessageBoxViewModel;
            if (vm != null)
            {
                vm.Cancel();
            }
        }
   
    }
}

    

