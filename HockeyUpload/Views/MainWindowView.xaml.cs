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
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        public MainWindowView()
        {
            InitializeComponent();
            this.MyFlyout.IsOpenChanged += MyFlyout_IsOpenChanged;
        }

        void MyFlyout_IsOpenChanged(object sender, EventArgs e)
        {
            if (this.MyFlyout.IsOpen)
            {
                this.MyFlyout.Width = this.Width - 125;
            }
            
        }

      
    }
}
