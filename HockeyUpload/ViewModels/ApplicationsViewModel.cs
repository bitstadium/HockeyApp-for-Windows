using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;

namespace HockeyApp.AppLoader.ViewModels
{
    public class ApplicationsViewModel : ViewModelBase
    {
        public ApplicationsViewModel(UserConfiguration configurationToSelect = null)
        {
            UserConfigurationViewModel toSelect = null;
            
            this.UserConfigurations = new ObservableCollection<UserConfigurationViewModel>();
            this.UserConfigurations.CollectionChanged += (a, b) => {
                NotifyOfPropertyChange(() => this.HasConfigurations);
            };
            foreach (UserConfiguration current in ConfigurationStore.Instance.UserConfigurations)
            {
                UserConfigurationViewModel newUCVM = new UserConfigurationViewModel(current);
                this.UserConfigurations.Add(newUCVM);
                if (current.Equals(configurationToSelect)) { toSelect = newUCVM; }
            }

            if (toSelect == null)
            {
                toSelect = this.UserConfigurations.FirstOrDefault(p => p.IsDefaultConfiguration);
            }
            if (toSelect == null && this.UserConfigurations.Count > 0)
            {
                toSelect = this.UserConfigurations[0];
            }
            if (toSelect != null) { 
                this.SelectedUserConfiguration = toSelect ;
            }
        }

        public ICollectionView Apps { get; set; }
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
        
        public ObservableCollection<UserConfigurationViewModel> UserConfigurations
        {
            get;
            private set;
        }

        public bool HasConfigurations { get { return this.UserConfigurations != null && this.UserConfigurations.Count > 0; } }

        private bool _cboChangedByUser = true;
        public async void SelectedUserConfiguration_OnSelectionChanged()
        {
            if (this._cboChangedByUser)
            {

                if (this.SelectedUserConfiguration == null)
                {
                    this.Apps = null;                    
                }
                else
                {
                    try
                    {
                        await this.RefreshApps();
                    }
                    catch 
                    {
                        this._cboChangedByUser = false;
                        this.SelectedUserConfiguration = this._oldSelectedUserConfiguration;
                    }
                }
            }
            else
            {
                this._cboChangedByUser = true;
            }
        }

        private UserConfigurationViewModel _oldSelectedUserConfiguration = null;
        private UserConfigurationViewModel _selectedUserConfiguration = null;
        public UserConfigurationViewModel SelectedUserConfiguration
        {
            get { return this._selectedUserConfiguration; }
            set
            {
                this._oldSelectedUserConfiguration = this._selectedUserConfiguration;
                this._selectedUserConfiguration = value;
               
                NotifyOfPropertyChange(() => this.SelectedUserConfiguration);
                this.SelectedUserConfiguration_OnSelectionChanged();
            }
        }

        public async Task RefreshApps()
        {
            Exception exThrown = null;
            var wm = Caliburn.Micro.IoC.Get<IWindowManager>();
            ProgressDialogController pdc = await  wm.ShowProgressAsync("Loading...", "Please wait - we are loading you apps...");
            try
            {
                AppInfoEnvelope envelope = await AppInfoEnvelope.Load(this._selectedUserConfiguration.UserConfiguration);
                
                List<AppConfigViewModel> list = new List<AppConfigViewModel>();
                if (envelope != null && envelope.Apps != null)
                {
                    foreach (AppInfo current in envelope.Apps)
                    {
                        list.Add(new AppConfigViewModel(current, this.SelectedUserConfiguration.UserConfiguration));
                    }
                }

                this.Apps = CollectionViewSource.GetDefaultView(list);
                this.Apps.SortDescriptions.Add(new SortDescription("Platform", ListSortDirection.Ascending));
                this.Apps.Filter = (x) => { return (x as AppConfigViewModel).Title.ToUpper().Contains(this.FilterString.ToUpper())
                    || (x as AppConfigViewModel).Platform.ToUpper().Contains(this.FilterString.ToUpper());
                };
                NotifyOfPropertyChange(() => this.Apps);
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
                await wm.ShowMessageAsync("Error", "An error occurred:\n" + exThrown.Message, MessageDialogStyle.Affirmative, mySettings);
                throw exThrown;
            }

        }

        public async void DeleteConfiguration()
        {
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "delete",
                NegativeButtonText = "cancel"
            };
            IWindowManager wm = IoC.Get<IWindowManager>();
            if (await wm.ShowMessageAsync("Delete configuration", "Deletion of a userconfiguration will delete all local stored app informations.\nDo you really want to delete?",
                MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                ConfigurationStore.Instance.UserConfigurations.Remove(this.SelectedUserConfiguration.UserConfiguration);
                ConfigurationStore.Instance.Save();
                this.UserConfigurations.Remove(this.SelectedUserConfiguration);
                if (this.UserConfigurations.Count > 0)
                {
                    this.SelectedUserConfiguration = this.UserConfigurations[0];
                }
                else
                {
                    this.SelectedUserConfiguration = null;
                }
            }
        }


        public void SetDefaultConfiguration()
        {
            ConfigurationStore.Instance.SetDefaultUserConfiguration(this.SelectedUserConfiguration.UserConfiguration);
            ConfigurationStore.Instance.Save();
            this.UserConfigurations.ToList().ForEach(p => p.NotifyOfPropertyChange(() => p.IsDefaultConfiguration));
        }

        public bool CanSetDefaultConfiguration
        {
            get
            {
                return this.SelectedUserConfiguration != null && !this.SelectedUserConfiguration.IsDefaultConfiguration;
            }
        }

        public void ShowFeedbackFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowFeedbackFlyout();
        }


        public void ShowAboutFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowAboutFlyout();
        }

        public void ShowGeneralConfigurationFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowGeneralConfigurationFlyout();
        }

        public void ShowAddUserConfigurationFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowAddUserConfigurationFlyout();
        }

    }
}