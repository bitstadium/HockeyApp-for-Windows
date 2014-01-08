using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.ViewModels
{
    public class UserConfigurationViewModel : ViewModelBase, IDataErrorInfo
    {

        private Configuration _configuration;
        private UserConfiguration _userConfiguration;
        public UserConfigurationViewModel(UserConfiguration userConfiguration)
        {
            this._configuration = IoC.Get<Configuration>();
            this._userConfiguration = userConfiguration;
            this.Cancel();
        }

        public UserConfiguration UserConfiguration { get { return this._userConfiguration; } }
        public bool IsNew { get {return this._userConfiguration == null; } }
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

        private bool _wasChanged = false;
        public bool WasChanged
        {
            get { return this._wasChanged; }
            set
            {
                this._wasChanged = value;
                NotifyOfPropertyChange(() => this.WasChanged);
                NotifyOfPropertyChange(() => this.CanSave);
                NotifyOfPropertyChange(() => this.CanCancel);
            }
        }

        private string _apiBase = "";
        public string ApiBase
        {
            get
            {
                return this._apiBase;
            }
            set
            {
                this._apiBase = value;
                this.WasChanged = true;
                this.NotifyOfPropertyChange(() => this.ApiBase);
                this.NotifyOfPropertyChange(() => this.CanLookupAPIToken);
                
            }
        }

        private bool _isDefaultConfiguration = false;
        public bool IsDefaultConfiguration
        {
            get { return this._isDefaultConfiguration; }
            set
            {
                this._isDefaultConfiguration = value;
                this.WasChanged = true;
            }
        }

        private string _configurationName = "";
        public string ConfigurationName
        {
            get { return this._configurationName; }
            set
            {
                this._configurationName = value;
                this.WasChanged = true;
                this.NotifyOfPropertyChange(() => this.ConfigurationName);
            }
        }

        private string _userToken;
        public string UserToken
        {
            get { return this._userToken; }
            set
            {
                this._userToken = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.UserToken);
            }
        }

     

        #region Commands
        public void Save()
        {
            
            if (this.IsNew)
            {
                this._userConfiguration = UserConfiguration.CreateNew(this.ConfigurationName);
            }

            this._userConfiguration.UserToken = this.UserToken;
            if (!this.ApiBase.EndsWith("/")) { this.ApiBase += "/"; }
            this._userConfiguration.ApiBase = this.ApiBase;

            if (this._userConfiguration.IsDefault && !this.IsDefaultConfiguration)
            {
                this._configuration.SetDefaultUserConfiguration(null);
            }
            else if (!this._userConfiguration.IsDefault && this.IsDefaultConfiguration)
            {
                this._configuration.SetDefaultUserConfiguration(this._userConfiguration);
            }

            this._configuration.Save();
            this.WasChanged = false;
            NotifyOfPropertyChange("");
            base.Close();
        }

        public bool CanSave
        {
            get { return this.WasChanged && this.IsValid; }
        }

        public void Cancel()
        {
            if (this.IsNew)
            {
                this.ConfigurationName = "New Configuration";
                this.ApiBase = HockeyApp.AppLoader.Properties.Settings.Default.DefaultApiBase;
                this.IsDefaultConfiguration = false;
                this.UserToken = "";
            }
            else
            {
                this.ConfigurationName = this._userConfiguration.ConfigurationName;
                this.ApiBase = this._userConfiguration.ApiBase;
                this.IsDefaultConfiguration = this._userConfiguration.IsDefault;
                this.UserToken = this._userConfiguration.UserToken;
            }
            this.WasChanged = false;
            base.Close();
        }

        public bool CanCancel { get { return true; } }

        public void LookupAPIToken()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            LookupApiTokenViewModel vm = new LookupApiTokenViewModel(this.ApiBase);

            if (wm.ShowDialog(vm).GetValueOrDefault(false))
            {
                this.UserToken = vm.SelectedApiToken.Token;
            }
        }

        public bool CanLookupAPIToken { get { return this.ApiBase != null && this.ApiBase.Length > 0; } }
        #endregion

        #region IDataErrorInfo


        private static readonly string[] ValidatedProperties ={
            "ConfigurationName",
            "ApiBase",
            "UserToken"
        };

        private bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (this[property] != null) // there is an error
                        return false;
                }
                return true;
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string error = null;
                switch (columnName)
                {
                    case "ConfigurationName":
                        if (this.IsNew && (String.IsNullOrWhiteSpace(this.ConfigurationName) || this._configuration.IsConfigurationNameUsed(this.ConfigurationName)))
                        {
                            error = "Name already in use!";
                        }
                        break;
                    case "ApiBase":
                        if (this.ApiBase == null || this.ApiBase.Length == 0)
                        {
                            error = "Please insert an ApiBase for the HockeyAppServer!";
                        }
                        break;
                    case "UserToken":
                        if (this.UserToken == null || this.UserToken.Length == 0)
                        {
                            error = "Please insert an UserToken!";
                        }
                        break;
                }
                return error;
            }
        }
        #endregion
    }
}
