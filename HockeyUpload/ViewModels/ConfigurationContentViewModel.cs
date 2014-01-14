using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HockeyApp.AppLoader.ViewModels
{
    public class ConfigurationContentViewModel:ViewModelBase
    {
        //TODO: Rausschmeißen
        public ConfigurationContentViewModel()
        {
            this.AppsView = IoC.Get<ApplicationsViewModel>();
            this.SettingsView = IoC.Get<ConfigurationViewModel>();
            this.FeedbackView = IoC.Get<FeedbackViewModel>();
        }
        public ViewModelBase AppsView{get;private set;}
        public ViewModelBase SettingsView{get;private set;}
        public ViewModelBase FeedbackView{get;private set;}

    }
}
