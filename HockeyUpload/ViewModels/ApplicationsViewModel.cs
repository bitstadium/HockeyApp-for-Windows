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
        public ApplicationsViewModel()
        {
            var vm = IoC.Get<ConfigurationViewModel>();
            this.UserConfigurations = vm.Configurations;
            var defConf = vm.Configurations.FirstOrDefault(p => p.IsDefaultConfiguration);
            if (defConf != null)
            {
                this.SelectedUserConfiguration = defConf;
                this.SelectedUserConfiguration_OnSelectionChanged();
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
    }
}