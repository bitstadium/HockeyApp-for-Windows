using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.Util;
using HockeyApp.AppLoader.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HockeyApp.AppLoader.Extensions;
using System.Net;
using System.Windows.Data;

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
            Task t = this.LoadGravatar();
        }

        public async Task RefreshAvatarAsync()
        {
            await LoadGravatar();
        }
        private async Task LoadGravatar()
        {
            this.Gravatar = await GravatarHelper.LoadGravatar(this._userConfiguration.GravatarHash);
            NotifyOfPropertyChange(() => this.Gravatar);
        }

        public UserConfiguration UserConfiguration { get { return this._userConfiguration; } }
        public ICollectionView Apps{get; private set;}
        private string _filterString = "";
        public string FilterString
        {
            get
            {
                return this._filterString;
            }
            set
            {
                this._filterString = value;
                NotifyOfPropertyChange(() => this.FilterString);
                this.Apps.Refresh();
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
            set { 
                ConfigurationStore.Instance.SetDefaultUserConfiguration(value?this.UserConfiguration:null);
                NotifyOfPropertyChange(() => this.IsDefaultConfiguration);
            }
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

        public bool IsInvalid
        {
            get { return this._userConfiguration.IsInvalid; }
            set
            {
                this._userConfiguration.IsInvalid = value;
                ConfigurationStore.Instance.Save();
                NotifyOfPropertyChange(() => this.IsInvalid);
            }
        }

        public bool AppsLoaded { get {return this.Apps != null; } }

        public async Task RefreshApps()
        {
            if (this.IsInvalid) { return; }
            Exception exThrown = null;
            var wm = Caliburn.Micro.IoC.Get<IWindowManager>();
            List<AppConfigViewModel> list = new List<AppConfigViewModel>();

            ProgressDialogController pdc = await wm.ShowProgressAsync("Please wait...", "Loading apps and settings");
            try
            {
                AppInfoEnvelope envelope = await AppInfoEnvelope.Load(this.UserConfiguration);


                if (envelope != null && envelope.Apps != null)
                {
                    foreach (AppInfo current in envelope.Apps)
                    {
                        list.Add(new AppConfigViewModel(current, this.UserConfiguration));
                    }
                }


                this.IsInvalid = false;
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }
            await pdc.CloseAsync();

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
            };
            if (exThrown != null)
            {
                if (exThrown is WebException && ((WebException)exThrown).Status == WebExceptionStatus.ProtocolError)
                {
                    await wm.ShowMessageAsync("Error", "API key invalid. Please choose another API key for this configuration!", MessageDialogStyle.Affirmative, mySettings);
                    this.IsInvalid = true;
                }
                else
                {
                    await wm.ShowMessageAsync("Error", "An error occurred:\n" + exThrown.Message, MessageDialogStyle.Affirmative, mySettings);
                    throw exThrown;
                }
            }

            this.Apps = CollectionViewSource.GetDefaultView(list);
            this.Apps.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            this.Apps.SortDescriptions.Add(new SortDescription("ReleaseType", ListSortDirection.Ascending));

            this.Apps.Filter = (x) =>
            {
                return (x as AppConfigViewModel).Title.ToUpper().Contains(this.FilterString.ToUpper())
                    || (x as AppConfigViewModel).Platform.ToUpper().Contains(this.FilterString.ToUpper());
            };

            this.Apps.MoveCurrentToFirst();
            NotifyOfPropertyChange(() => this.Apps);

        }


    }
}
