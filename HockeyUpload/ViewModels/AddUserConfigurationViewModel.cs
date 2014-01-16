using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Media;
using HockeyApp.AppLoader.Util;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace HockeyApp.AppLoader.ViewModels
{
    public class AddUserConfigurationViewModel : ViewModelBase, IDataErrorInfo
    {
        public AddUserConfigurationViewModel()
        {
            base.DisplayName = "Add new configuration";
            this.ApiBase = HockeyApp.AppLoader.Properties.Settings.Default.DefaultApiBase;

            string configName = "Name of new Configuration";
            string suffix = "";
            int i = 0;
            while (ConfigurationStore.Instance.IsConfigurationNameUsed(configName + suffix))
            {
                suffix = "_" + ++i;
            }
            this.ConfigurationName = configName + suffix;
        }

        public UserConfiguration NewUserConfiguration { get; private set; }
        public string ApiBase { get; set; }
        public string ConfigurationName { get; set; }
        public bool IsDefault { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private string _name = "";
        public string Name
        {
            get { return this._name; }
            private set
            {
                this._name = value;
                NotifyOfPropertyChange(() => this.Name);
            }
        }


        public ImageSource Gravatar { get; private set; }
        
        private void LoadGravatar()
        {
            NotifyOfPropertyChange(() => this.Gravatar);
            Task<BitmapImage> t = GravatarHelper.LoadGravatar(this.GravatarHash);
            Task t1 = t.ContinueWith(p => {
                this.Gravatar = t.Result;
                NotifyOfPropertyChange(() => this.Gravatar);
            });
        }
        private string _gravatarHash = "";
        public string GravatarHash
        {
            get { return this._gravatarHash; }
            set
            {
                this._gravatarHash = value;
                NotifyOfPropertyChange(() => this.GravatarHash);
                LoadGravatar();
            }
        }

        public List<ApiToken> ApiTokens { get; private set; }
        private ApiToken _selectedApiToken = null;
        public ApiToken SelectedApiToken
        {
            get { return this._selectedApiToken; }
            set
            {
                this._selectedApiToken = value;
                NotifyOfPropertyChange(() => this.SelectedApiToken);
                //NotifyOfPropertyChange(() => this.CanOK);
            }
        }

        public async Task Login()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            Exception exThrown = null;
            this.Name = "";
            this.GravatarHash = "";
            ProgressDialogController pdc = await wm.ShowProgressAsync("Login", "Please wait - we are loading your userdata...");
            this.ApiTokens = null;
            this.SelectedApiToken = null;
            
            try
            {
                ApiTokenEnvelope envelope = await ApiTokenEnvelope.Load(this.ApiBase, this.Username, this.Password);
                this.ApiTokens = envelope.Tokens.Where(p => p.Rights.Equals("0") || p.Rights.Equals("1")).ToList();
                //this.ApiTokens.Insert(0, new CreateNewApiToken());
                NotifyOfPropertyChange(() => this.ApiTokens);

                this.Name = envelope.Name;
                this.GravatarHash = envelope.GravatarHash;
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }
            await pdc.CloseAsync();
            if (exThrown != null){
                await wm.ShowSimpleMessageAsync("Error", "An error occured:\n" + exThrown.Message);
            }
            else
            {
                if (this.ApiTokens.Count == 0)
                {
                    MetroDialogSettings settings = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "create api token",
                        NegativeButtonText = "cancel"
                    };
                    if (await wm.ShowMessageAsync("Create API Token?", "We couldn't find an API token with write-access rights. Do you want to create one?", MessageDialogStyle.AffirmativeAndNegative, settings)
                        == MessageDialogResult.Affirmative)
                    {
                        ApiToken newToken = await ApiTokenEnvelope.CreateNewToken(this.ApiBase, this.Username, this.Password);
                        await this.Login();
                        ApiToken toSelect = this.ApiTokens.FirstOrDefault(p => p.Token.Equals(newToken.Token));
                        if (toSelect != null)
                        {
                            this.SelectedApiToken = toSelect;
                        }
                        else
                        {
                            this.SelectedApiToken = this.ApiTokens[0];
                        }
                    }
                }
            }
        }

        public void OK()
        {
            UserConfiguration newUC = UserConfiguration.CreateNew(this.ConfigurationName);
            newUC.ApiBase = this.ApiBase;
            newUC.Username = this.Username;
            newUC.UserToken = this.SelectedApiToken.Token;
            newUC.GravatarHash = this.GravatarHash;
            this.NewUserConfiguration = newUC;
            if (IsDefault) { ConfigurationStore.Instance.SetDefaultUserConfiguration(newUC); }
            ConfigurationStore.Instance.Save();
            
            this.Close();
        }

        public bool CanOK
        {
            get
            {
                return this["ConfigurationName"] == null;
            }
        }
        public string Error
        {
            get{ return string.Empty; } 
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ConfigurationName")
                {
                    if (string.IsNullOrWhiteSpace(this.ConfigurationName))
                    {
                        return "Please enter a name for the configuration!";
                    }else if(ConfigurationStore.Instance.IsConfigurationNameUsed(this.ConfigurationName)){
                        return "Name is already used!";
                    }
                }

                return null;
            }
        }
    }
}
