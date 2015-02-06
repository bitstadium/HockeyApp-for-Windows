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
using HockeyApp.AppLoader.ViewModels;

namespace HockeyApp.AppLoader.Views
{
    /// <summary>
    /// Interaction logic for FeedbackThreadView.xaml
    /// </summary>
    public partial class FeedbackThreadView : UserControl
    {
        public FeedbackThreadView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.itemsControl.Items.Count > 0){
                object item = this.itemsControl.Items[this.itemsControl.Items.Count - 1];
                var element = this.itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (element != null)
                {
                    element.BringIntoView();
                }
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            (((Hyperlink)sender).DataContext as FeedbackAttachmentViewModel).OpenAttachment();
        }
    }
}
