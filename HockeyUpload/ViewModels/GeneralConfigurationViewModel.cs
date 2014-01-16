using HockeyApp.AppLoader.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HockeyApp.AppLoader.ViewModels
{
    public class GeneralConfigurationViewModel:ViewModelBase
    {

        public GeneralConfigurationViewModel()
        {
            base.DisplayName = "General";
            this.OpenAPPTFileCommand = new RelayCommand<object>(p => {
                OpenAPPTFile();
            });
        }
        public string PathToAAPT
        {
            get { return HockeyApp.AppLoader.Properties.Settings.Default.AAPTPath; }
            set
            {
                HockeyApp.AppLoader.Properties.Settings.Default.AAPTPath = value;
                HockeyApp.AppLoader.Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => this.PathToAAPT);
            }
        }

        public void OpenAPPTFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Executables | *.exe";
            dlg.Multiselect = false;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                this.PathToAAPT = dlg.FileName;
            }
        }

        public ICommand OpenAPPTFileCommand { get; private set; }
    }
}
