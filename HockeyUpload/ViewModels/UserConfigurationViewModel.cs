using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.Util;
using HockeyApp.AppLoader.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace HockeyApp.AppLoader.ViewModels
{
    public class UserConfigurationViewModel : ViewModelBase
    {

        private ConfigurationStore _configuration;
        private UserConfiguration _userConfiguration;
        public UserConfigurationViewModel(UserConfiguration userConfiguration)
        {
            this._configuration = IoC.Get<ConfigurationStore>();
            this._userConfiguration = userConfiguration;
            this.LoadGravatar();
        }

        private async void LoadGravatar()
        {
            this.Gravatar = await GravatarHelper.LoadGravatar(this._userConfiguration.GravatarHash);
            NotifyOfPropertyChange(() => this.Gravatar);
        }

        public UserConfiguration UserConfiguration { get { return this._userConfiguration; } }
        public List<AppInfo> AppInfos
        {
            get
            {
                if (this._userConfiguration.AppInfos == null) { this._userConfiguration.AppInfos = new List<AppInfo>(); }
                return this._userConfiguration.AppInfos;
            }
            set
            {
                this._userConfiguration.AppInfos = value;
            }
        }

        
        public string ApiBase
        {
            get
            {
                return this._userConfiguration.ApiBase;
            }
        }

        public bool IsDefaultConfiguration
        {
            get { return this.UserConfiguration.IsDefault; }
        }

        public string ConfigurationName
        {
            get { return this._userConfiguration.ConfigurationName; }
        }

        public string UserName
        {
            get
            {
                return this._userConfiguration.Username;
            }
        }

        public string UserToken
        {
            get { return this.UserConfiguration.UserToken; }
        }

        public ImageSource Gravatar
        {
            get;private set;
        }

    }
}
