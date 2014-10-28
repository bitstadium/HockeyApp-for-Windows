using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.Util;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HockeyApp.AppLoader.Extensions;

namespace HockeyApp.AppLoader.ViewModels
{
    public class EditUserConfigurationViewModel: ViewModelBase
    {

        private UserConfigurationViewModel _userConfigurationToEdit;
        public EditUserConfigurationViewModel(UserConfigurationViewModel userConfiguration)
        {
            base.DisplayName = "Edit configuration";
            this._userConfigurationToEdit = userConfiguration;

            this.ApiBase = this._userConfigurationToEdit.ApiBase;
            this.ConfigurationName = this._userConfigurationToEdit.ConfigurationName;
            this.Username = this._userConfigurationToEdit.UserName;
            this.Gravatar = this._userConfigurationToEdit.Gravatar;
        }

        
        public string ApiBase
        {
            get;
            private set;
        }

        public string ConfigurationName
        {
            get { return this._userConfigurationToEdit.ConfigurationName; }
            set
            {
                this._userConfigurationToEdit.UserConfiguration.ConfigurationName = value;
                ConfigurationStore.Instance.Save();
                NotifyOfPropertyChange(() => this.CanLogin);
                this._userConfigurationToEdit.NotifyOfPropertyChange(()=>this._userConfigurationToEdit.ConfigurationName);
            }
        }

        public bool IsDefault
        {
            get
            {
                return this._userConfigurationToEdit.IsDefaultConfiguration;
            }
            set
            {
                this._userConfigurationToEdit.IsDefaultConfiguration = value;
            }
        }

        private string _userName = "";
        public string Username
        {
            get
            {
                return this._userName;
            }
            set
            {
                this._userName = value;
                NotifyOfPropertyChange(() => this.Username);
                NotifyOfPropertyChange(() => this.CanLogin);
            }
        }

        private string _password = "";
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
                NotifyOfPropertyChange(() => this.Password);
                NotifyOfPropertyChange(() => this.CanLogin);
            }
        }

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

        private List<ApiToken> _apiTokens = null;
        public List<ApiToken> ApiTokens
        {
            get { return this._apiTokens; }
            set
            {
                this._apiTokens = value;
                NotifyOfPropertyChange(() => this.ApiTokens);
            }
        }

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

        public bool CanLogin
        {
            get
            {
                return !String.IsNullOrWhiteSpace(this.ConfigurationName)
                    && !String.IsNullOrWhiteSpace(this.Username)
                    && !String.IsNullOrWhiteSpace(this.Password);
            }
        }

        private bool _showGravatar = false;
        public bool ShowGravatar
        {
            get { return this._showGravatar; }
            set
            {
                this._showGravatar = value;
                NotifyOfPropertyChange(() => this.ShowGravatar);
            }
        }


        private void LoadGravatar()
        {
            NotifyOfPropertyChange(() => this.Gravatar);
            Task<BitmapImage> t = GravatarHelper.LoadGravatar(this.GravatarHash);
            Task t1 = t.ContinueWith(p =>
            {
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

        public async Task Login()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();

            this.IsError = false;
            this.Name = "";
            this.ShowGravatar = false;
            this.ApiTokens = null;
            Exception exThrown = null;
            MetroDialogSettings s = new MetroDialogSettings()
            {
                UseAnimations = false
            };
            ProgressDialogController pdc = await wm.ShowProgressAsync("Please wait...", "Loading user data", false, s);
            this.ApiTokens = null;
            this.SelectedApiToken = null;

            try
            {
                ApiTokenEnvelope envelope = await ApiTokenEnvelope.Load(this.ApiBase, this.Username, this.Password);
                this.ApiTokens = envelope.Tokens.Where(p => p.Rights.Equals("0") || p.Rights.Equals("1")).ToList();
                //this.ApiTokens.Insert(0, new CreateNewApiToken());
                NotifyOfPropertyChange(() => this.ApiTokens);
                NotifyOfPropertyChange(() => this.IsTokenAvailable);

                this.Name = envelope.Name;
                this.GravatarHash = envelope.GravatarHash;
                this.ShowGravatar = true;
            }
            catch (Exception ex)
            {
                exThrown = ex;
                NotifyOfPropertyChange(() => this.IsTokenAvailable);
                this.ErrorMessage = "Login failed:\n" + ex.Message;
                this.IsError = true;
            }

            await pdc.CloseAsync();

            if (exThrown != null)
            {
                //await wm.ShowSimpleMessageAsync("An error ocurred", exThrown.Message);
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

        private bool _isError = false;
        public bool IsError
        {
            get { return this._isError; }
            private set
            {
                this._isError = value;
                NotifyOfPropertyChange(() => this.IsError);
                NotifyOfPropertyChange(() => this.ErrorMessage);
            }
        }
        public string ErrorMessage { get; private set; }
        public bool IsTokenAvailable { get { return this.ApiTokens != null && this.ApiTokens.Count > 0; } }

        public bool IsCanceled { get; private set; }
        public async void OK()
        {

            this._userConfigurationToEdit.UserConfiguration.Username = this.Username;
            this._userConfigurationToEdit.UserConfiguration.UserToken = this.SelectedApiToken.Token;
            this._userConfigurationToEdit.UserConfiguration.GravatarHash = this.GravatarHash;
            if (IsDefault) { ConfigurationStore.Instance.SetDefaultUserConfiguration(this._userConfigurationToEdit.UserConfiguration); }
            ConfigurationStore.Instance.Save();
            this._userConfigurationToEdit.NotifyOfPropertyChange("");
            this.Close();
            await this._userConfigurationToEdit.RefreshAvatarAsync();
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
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ConfigurationName")
                {
                    if (!string.IsNullOrWhiteSpace(this.ConfigurationName) && ConfigurationStore.Instance.IsConfigurationNameUsed(this.ConfigurationName))
                    {
                        return "Name is already used!";
                    }
                }

                return null;
            }
        }
    }
}
